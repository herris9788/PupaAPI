using Org.BouncyCastle.Security;
using System.Collections;
using System.Security.Cryptography;
using System.Text;

namespace Pupa.Helpers
{
    public static class Blowfish
    {
        private const int Iterations = 1000;

        public static byte[] HashPassword(string password)
        {
            using (var md5 = MD5.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = passwordBytes;

                for (int i = 0; i < Iterations; i++)
                {
                    hash = md5.ComputeHash(hash);
                }
                return hash;
            }
        }

        public static bool VerifyPassword(string password, byte[] storedHash)
        {
            byte[] computedHash = HashPassword(password);
            return StructuralComparisons.StructuralEqualityComparer.Equals(computedHash, storedHash);
        }

        // Helper untuk mengonversi byte array ke string hex (opsional)
        public static string ByteArrayToHexString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            return hex.ToString();
        }

        // Helper untuk mengonversi string hex ke byte array (opsional)
        public static byte[] HexStringToByteArray(string hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }

        public static string test(string p)
        {
            byte[] hash = HashPassword(p);
            return ByteArrayToHexString(hash);
        }

        //public static void Main(string[] args)
        //{
        //    string originalPassword = "mysecretpassword";
        //    byte[] hash = HashPassword(originalPassword);

        //    Console.WriteLine($"Hash (Hex) without salt: {ByteArrayToHexString(hash)}");

        //    string passwordToVerify = "mysecretpassword";
        //    bool isMatch = VerifyPassword(passwordToVerify, hash);
        //    Console.WriteLine($"Password '{passwordToVerify}' matches: {isMatch}");

        //    string wrongPassword = "wrongpassword";
        //    bool isWrongMatch = VerifyPassword(wrongPassword, hash);
        //    Console.WriteLine($"Password '{wrongPassword}' matches: {isWrongMatch}");
        //}
    }
}
