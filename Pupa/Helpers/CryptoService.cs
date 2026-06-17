using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Pupa.Helpers
{
    public class CryptoService
    {
        public static byte[] ToUnicodeBytes(string s) => new UnicodeEncoding().GetBytes(s);

        public static Guid ComputeGuid(string pwd) => new Guid(new MD5CryptoServiceProvider().ComputeHash(Encoding.ASCII.GetBytes(pwd)));

        public static string FromUnicodeBytes(byte[] buffer) => new UnicodeEncoding().GetString(buffer);

        public static byte[] ComputePasswordHash(string password)
        {
            if (password.Length == 0)
                return null;
            byte[] unicodeBytes = ToUnicodeBytes(password);
            byte[] hash = new MD5CryptoServiceProvider().ComputeHash(unicodeBytes);
            return new SHA512Managed().ComputeHash(new TripleDESCryptoServiceProvider().CreateEncryptor(hash, hash).TransformFinalBlock(unicodeBytes, 0, unicodeBytes.Length));
        }
    }
}
