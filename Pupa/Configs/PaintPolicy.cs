namespace Pupa.Configs
{
    /// <summary>
    /// Static paint-requisition rules. Each vessel group maps to the exact set of
    /// T06.001 item codes it is allowed to order. Source: T06.001 - PAINT requisition guide.
    /// </summary>
    public static class PaintPolicy
    {
        public const string PaintItemCodePrefix = "T06";

        private static readonly HashSet<string> _alkydGroupCodes =
            new(StringComparer.OrdinalIgnoreCase) { "TK1", "TK2", "TK3", "TK5", "TK6", "ST", "CC", "BC" };

        private static readonly HashSet<string> _tugTowingGroupCodes =
            new(StringComparer.OrdinalIgnoreCase) { "TB", "TG" };

        private static readonly Dictionary<string, IReadOnlyList<string>> _allowedCodes = new()
        {
            ["ALKYD"] = new[]
            {
                "T06.001.0004","T06.001.0024","T06.001.0023",
                "T06.001.00149","T06.001.00145","T06.001.00148","T06.001.00146","T06.001.00147",
                "T06.001.0005","T06.001.0126","T06.001.0012","T06.001.0013",
                "T06.001.0015","T06.001.0118","T06.001.0020","T06.001.0021",
                "T06.001.0120","T06.001.0002","T06.001.0022","T06.001.0025",
                "T06.001.00178","T06.001.00176",
            },
            ["ACRYLIC"] = new[]
            {
                "T06.001.0024","T06.001.0023",
                "T06.001.00149","T06.001.00145","T06.001.00148","T06.001.00146","T06.001.00147",
                "T06.001.0026","T06.001.00140","T06.001.0057","T06.001.0053",
                "T06.001.0054","T06.001.0114","T06.001.00132","T06.001.0028",
                "T06.001.0051","T06.001.0029","T06.001.0050","T06.001.0025",
                "T06.001.00178","T06.001.00177",
            },
            ["SPECIAL_GAS_OMEGA"] = new[]
            {
                "T06.001.0024","T06.001.0023",
                "T06.001.00149","T06.001.00145","T06.001.00148","T06.001.00146","T06.001.00147",
                "T06.001.00142",
                "T06.001.0026","T06.001.00140","T06.001.0057","T06.001.0053",
                "T06.001.0054","T06.001.0114","T06.001.00132","T06.001.0028",
                "T06.001.0051","T06.001.0029","T06.001.0050","T06.001.0025",
                "T06.001.00178","T06.001.00177",
            },
            ["SPECIAL_GAS_DAHLIA"] = new[]
            {
                "T06.001.0024","T06.001.0023",
                "T06.001.00149","T06.001.00145","T06.001.00148","T06.001.00146","T06.001.00147",
                "T06.001.00198",
                "T06.001.0026","T06.001.00140","T06.001.0057","T06.001.0053",
                "T06.001.0054","T06.001.0114","T06.001.00132","T06.001.0028",
                "T06.001.0051","T06.001.0029","T06.001.0050","T06.001.0025",
                "T06.001.00178","T06.001.00177",
            },
            ["SPECIAL_GAS_RAINBOW"] = new[]
            {
                "T06.001.0004","T06.001.0024","T06.001.0023",
                "T06.001.00149","T06.001.00145","T06.001.00148","T06.001.00146","T06.001.00147",
                "T06.001.0011",
                "T06.001.0005","T06.001.0126","T06.001.0012","T06.001.0013",
                "T06.001.0015","T06.001.0118","T06.001.0020","T06.001.0021",
                "T06.001.0120","T06.001.0002","T06.001.0022","T06.001.0025",
                "T06.001.00178","T06.001.00176",
            },
            ["SPECIAL_COMMODORE_ONE"] = new[]
            {
                "T06.001.0004","T06.001.0024","T06.001.0023",
                "T06.001.00149","T06.001.00145","T06.001.00148","T06.001.00146","T06.001.00147",
                "T06.001.00143",
                "T06.001.0005","T06.001.0126","T06.001.0012","T06.001.0013",
                "T06.001.0015","T06.001.0118","T06.001.0020","T06.001.0021",
                "T06.001.0120","T06.001.0002","T06.001.0022","T06.001.0025",
                "T06.001.00178","T06.001.00176",
            },
        };

        /// <summary>
        /// Resolves the paint group key from vessel name and group code.
        /// Returns null when the vessel has no known paint group (no filter applied).
        /// </summary>
        public static string? ResolveGroup(string vesselName, string? groupCode)
        {
            var name = (vesselName ?? "").ToUpperInvariant();

            if (name.Contains("GAS OMEGA"))       return "SPECIAL_GAS_OMEGA";
            if (name.Contains("GAS DAHLIA"))      return "SPECIAL_GAS_DAHLIA";
            if (name.Contains("GAS RAINBOW"))     return "SPECIAL_GAS_RAINBOW";
            if (name.Contains("COMMODORE ONE"))   return "SPECIAL_COMMODORE_ONE";
            if (name.Contains("INFINITY") || name.Contains("SEVEN TARGET")) return "ACRYLIC";

            if (_tugTowingGroupCodes.Contains(groupCode ?? "") ||
                name.Contains("TUG") || name.Contains("TOWING"))
                return "SPECIAL_GAS_RAINBOW";

            if (string.Equals(groupCode, "TK7", StringComparison.OrdinalIgnoreCase))
                return "ACRYLIC";

            if (_alkydGroupCodes.Contains(groupCode ?? ""))
                return "ALKYD";

            return null;
        }

        /// <summary>
        /// Returns the allowed item codes for a resolved group key, or null if unknown.
        /// </summary>
        public static IReadOnlyList<string>? AllowedCodes(string? groupKey)
        {
            if (groupKey == null) return null;
            return _allowedCodes.TryGetValue(groupKey, out var codes) ? codes : null;
        }
    }
}
