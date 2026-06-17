using System.Text.Json;
using System.Text.Json.Nodes;

namespace Pupa.Helpers
{
    public static class Filter
    {
        // Daftar kolom default yang digunakan untuk semua Global Search (Input String Tunggal & Array Grup OR)
        private static readonly string[] GlobalSearchColumnsDefault = new[] {
        "Company",
        "MRNumber",
        "Vessel",
        "Fleet",
        "ExternalRef"
    };

        // =================================================================================
        //                                  FUNGSI UTAMA
        // =================================================================================

        /// <summary>
        /// Fungsi utama untuk menerjemahkan filter DevExtreme (Array atau String) menjadi fragment SQL WHERE clause.
        /// </summary>
        /// <param name="filter">String filter dari DevExtreme, bisa Array JSON atau String tunggal.</param>
        /// <returns>Fragment string SQL, atau null jika tidak ada filter atau format tidak valid.</returns>
        public static string GenerateFilterSqlFragment(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return null;

            try
            {
                var jsonNode = JsonNode.Parse(filter);

                if (jsonNode is JsonArray filterArrayNode)
                {
                    // Input adalah Array, gunakan logika rekursif untuk Group/Simple
                    return ProcessFilterGroup(filterArrayNode);
                }
                else if (jsonNode is JsonValue filterValueNode)
                {
                    // Input adalah String Langsung yang di-quote (JSON Value)
                    var searchTerm = filterValueNode.GetValue<string>();
                    if (!string.IsNullOrWhiteSpace(searchTerm))
                    {
                        // Gunakan Fallback Global Search
                        return ProcessGlobalSearch(searchTerm);
                    }
                }
            }
            catch (JsonException)
            {
                // Fallback untuk string mentah yang gagal di-parse JSON
                return ProcessGlobalSearch(filter);
            }
            return null;
        }

        // =================================================================================
        //                                  LOGIKA GRUP DAN REKURSIF
        // =================================================================================

        /// <summary>
        /// Menganalisis Array Filter Grup: Mendeteksi pola Global Search OR atau memprosesnya secara literal.
        /// </summary>
        private static string ProcessFilterGroup(JsonArray filterArray)
        {
            if (filterArray.Count == 0) return null;

            string firstValue = null;
            bool isPureGlobalSearch = true;

            // 1. ITERASI UNTUK MENDETEKSI POLA PENCARIAN GLOBAL OR
            for (int i = 0; i < filterArray.Count; i++)
            {
                var element = filterArray[i];

                if (i % 2 == 1) // Elemen pada index ganjil harus operator logis
                {
                    if (element is JsonValue logicalValue && logicalValue.GetValue<string>().ToUpper() != "OR")
                    {
                        isPureGlobalSearch = false; // Bukan pola OR murni
                        break;
                    }
                }
                else // Elemen pada index genap adalah kondisi filter
                {
                    if (element is JsonArray conditionArray && conditionArray.Count == 3)
                    {
                        var value = conditionArray[2]?.GetValue<string>();

                        if (string.IsNullOrWhiteSpace(value)) { isPureGlobalSearch = false; break; }

                        if (firstValue == null)
                        {
                            firstValue = value;
                        }
                        else if (firstValue != value)
                        {
                            isPureGlobalSearch = false; // Nilai yang dicari berbeda
                            break;
                        }
                    }
                    else
                    {
                        isPureGlobalSearch = false; // Bukan array 3 elemen atau sub-grup nested
                        break;
                    }
                }
            }

            // --- IMPLEMENTASI KEPUTUSAN ---

            if (isPureGlobalSearch && !string.IsNullOrWhiteSpace(firstValue))
            {
                // KEPUTUSAN A: GLOBAL SEARCH DINAMIS/KONVERSI OR
                // Gunakan GlobalSearchColumnsDefault untuk CONCAT
                return ProcessGlobalSearch(firstValue);
            }
            else if (filterArray.Count == 3 && filterArray[1] is JsonValue val && val.GetValue<string>().ToUpper() != "OR")
            {
                // KEPUTUSAN B: FILTER SEDERHANA (3 elemen, bukan OR)
                return ProcessSimpleCondition(filterArray);
            }
            else
            {
                // KEPUTUSAN C: FILTER GRUP KUSTOM (Proses literal AND/OR/nested)
                return ProcessGroupAsLiteral(filterArray);
            }
        }

        /// <summary>
        /// Mengembalikan fragment SQL untuk filter grup yang kompleks/kustom secara literal (rekursif)
        /// </summary>
        private static string ProcessGroupAsLiteral(JsonArray filterArray)
        {
            var result = new List<string>();
            string logicalOperator = "AND";

            foreach (var element in filterArray)
            {
                if (element is JsonValue value)
                {
                    logicalOperator = value.GetValue<string>().ToUpper();
                }
                else if (element is JsonArray subArray)
                {
                    string subExpression;
                    if (subArray.Count == 3)
                    {
                        subExpression = ProcessSimpleCondition(subArray);
                    }
                    else
                    {
                        subExpression = ProcessGroupAsLiteral(subArray);
                    }

                    if (!string.IsNullOrWhiteSpace(subExpression))
                    {
                        if (result.Any()) result.Add(logicalOperator);
                        result.Add($"({subExpression})");
                    }
                }
            }
            return string.Join(" ", result);
        }
        // =================================================================================
        //                                  LOGIKA KONDISI
        // =================================================================================

        /// <summary>
        /// Memproses kondisi filter sederhana [field, operator, value] dan mengembalikan fragment SQL.
        /// Mengubah operator '=' dan 'contains' menjadi LIKE '%value%'.
        /// </summary>
        private static string ProcessSimpleCondition(JsonArray filterArray)
        {
            if (filterArray.Count != 3) return null;
            var type = filterArray[0]?.GetType();
            var f = filterArray[0];
            var field = f[0]?.GetValue<string>();
            var op = f[1]?.GetValue<string>();
            var value = f[2];

            if (string.IsNullOrWhiteSpace(field) || string.IsNullOrWhiteSpace(op)) return null;

            // Normalisasi Operator SQL: Operator '=' juga diarahkan ke LIKE untuk substring search
            var sqlOperator = op.ToLower() switch
            {
                "=" => "LIKE",
                "contains" => "LIKE",
                "startswith" => "LIKE",
                "endswith" => "LIKE",
                _ => op
            };

            // Normalisasi Nilai dan Quoting SQL
            string sqlValue;
            if (value is JsonValue jsonValue && jsonValue.GetValueKind() == JsonValueKind.String)
            {
                string stringValue = jsonValue.GetValue<string>();

                sqlValue = sqlOperator.ToLower() switch
                {
                    // Semua operator LIKE yang merupakan substring search (termasuk operator '=' yang kita ubah)
                    "like" when op.ToLower() == "contains" || op.ToLower() == "=" => $"'%{stringValue}%'",
                    "like" when op.ToLower() == "startswith" => $"'{stringValue}%'",
                    "like" when op.ToLower() == "endswith" => $"'%{stringValue}'",
                    _ => $"'{stringValue}'" // Fallback ke kutip tunggal
                };
            }
            else
            {
                sqlValue = value?.ToString(); // Angka atau Boolean
            }

            // Menggunakan alias 'a.' seperti yang Anda inginkan
            return $"a.{field} {sqlOperator} {sqlValue}";
        }

        /// <summary>
        /// Mengimplementasikan CONCAT Global Search menggunakan daftar kolom default.
        /// Digunakan untuk Input String Tunggal atau Array Grup OR.
        /// </summary>
        private static string ProcessGlobalSearch(string searchTerm)
        {
            var likeValue = $"'%{searchTerm}%'";

            // Membangun string CONCAT dengan kolom dari GlobalSearchColumnsDefault
            var concatExpression = string.Join(", ", GlobalSearchColumnsDefault.Select(f => $"a.{f}"));

            return $"CONCAT({concatExpression}) LIKE {likeValue}";
        }
    }
}
