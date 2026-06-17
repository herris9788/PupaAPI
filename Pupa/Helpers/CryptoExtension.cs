using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Pupa.Helpers
{
    /// <summary>
    /// Custom Build Encryption Extension
    /// </summary>
    /// 
    public class CryptoExtension
    {
        private static readonly string DEFAULT_HASH_ALGORITHM = "SHA1";

        private static readonly int DEFAULT_KEY_SIZE = 128;

        private static readonly int MAX_ALLOWED_SALT_LEN = 255;

        private static readonly int MIN_ALLOWED_SALT_LEN = 4;

        private static readonly int DEFAULT_MIN_SALT_LEN = MIN_ALLOWED_SALT_LEN;
        private static readonly int DEFAULT_MAX_SALT_LEN = 8;

        private readonly int minSaltLen = -1;
        private readonly int maxSaltLen = -1;

        private readonly ICryptoTransform encryptor = null;
        private readonly ICryptoTransform decryptor = null;

        public CryptoExtension(string passPhrase) : this(passPhrase, null)
        {

        }

        public CryptoExtension(string passPhrase, string initVector) : this(passPhrase, initVector, -1)
        {

        }

        public CryptoExtension(string passPhrase, string initVector, int minSaltLen) : this(passPhrase, initVector, minSaltLen, -1)
        {

        }

        public CryptoExtension(string passPhrase, string initVector, int minSaltLen, int maxSaltLen) : this(passPhrase, initVector, minSaltLen, maxSaltLen, -1)
        {

        }

        public CryptoExtension(string passPhrase, string initVector, int minSaltLen, int maxSaltLen, int keySize) : this(passPhrase, initVector, minSaltLen, maxSaltLen, keySize, null)
        {

        }

        public CryptoExtension(string passPhrase, string initVector, int minSaltLen, int maxSaltLen, int keySize, string hashAlgorithm) : this(passPhrase, initVector, minSaltLen, maxSaltLen, keySize, hashAlgorithm, null)
        {

        }

        public CryptoExtension(string passPhrase, string initVector, int minSaltLen, int maxSaltLen, int keySize, string hashAlgorithm, string saltValue) : this(passPhrase, initVector, minSaltLen, maxSaltLen, keySize, hashAlgorithm, saltValue, 1)
        {

        }

        public CryptoExtension(string passPhrase, string initVector, int minSaltLen, int maxSaltLen, int keySize, string hashAlgorithm, string saltValue, int passwordIterations)
        {
            if (minSaltLen < MIN_ALLOWED_SALT_LEN)
            {
                this.minSaltLen = DEFAULT_MIN_SALT_LEN;
            }
            else
            {
                this.minSaltLen = minSaltLen;
            }

            if (maxSaltLen < 0 || maxSaltLen > MAX_ALLOWED_SALT_LEN)
            {
                this.maxSaltLen = DEFAULT_MAX_SALT_LEN;
            }
            else
            {
                this.maxSaltLen = maxSaltLen;
            }

            if (keySize <= 0)
            {
                keySize = DEFAULT_KEY_SIZE;
            }

            if (hashAlgorithm == null)
            {
                hashAlgorithm = DEFAULT_HASH_ALGORITHM;
            }
            else
            {
                hashAlgorithm = hashAlgorithm.ToUpper().Replace("-", "");
            }

            byte[] initVectorBytes;

            if (initVector == null)
            {
                initVectorBytes = new byte[0];
            }
            else
            {
                initVectorBytes = Encoding.ASCII.GetBytes(initVector);
            }

            byte[] saltValueBytes;

            if (saltValue == null)
            {
                saltValueBytes = new byte[0];
            }
            else
            {
                saltValueBytes = Encoding.ASCII.GetBytes(saltValue);
            }

            var password = new PasswordDeriveBytes(passPhrase, saltValueBytes, hashAlgorithm, passwordIterations);

            byte[] keyBytes = password.GetBytes(keySize / 8);

            var symmetricKey = new RijndaelManaged
            {
                BlockSize = 128
            };

            if (initVectorBytes.Length == 0)
            {
                symmetricKey.Mode = CipherMode.ECB;
            }
            else
            {
                symmetricKey.Mode = CipherMode.CBC;
            }

            encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);
            decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);
        }

        public string Encrypt(string plainText)
        {
            return Encrypt(Encoding.UTF8.GetBytes(plainText));
        }

        public string Encrypt(byte[] plainTextBytes)
        {
            return Convert.ToBase64String(EncryptToBytes(plainTextBytes));
        }

        public byte[] EncryptToBytes(string plainText)
        {
            return EncryptToBytes(Encoding.UTF8.GetBytes(plainText));
        }

        public byte[] EncryptToBytes(byte[] plainTextBytes)
        {
            byte[] plainTextBytesWithSalt = AddSalt(plainTextBytes);

            var memoryStream = new MemoryStream();

            lock (this)
            {
                var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);

                cryptoStream.Write(plainTextBytesWithSalt, 0, plainTextBytesWithSalt.Length);

                cryptoStream.FlushFinalBlock();

                byte[] cipherTextBytes = memoryStream.ToArray();

                memoryStream.Close();
                cryptoStream.Close();

                return cipherTextBytes;
            }
        }

        public string Decrypt(string cipherText)
        {
            return Decrypt(Convert.FromBase64String(cipherText));
        }

        public string Decrypt(byte[] cipherTextBytes)
        {
            return Encoding.UTF8.GetString(DecryptToBytes(cipherTextBytes));
        }

        public byte[] DecryptToBytes(string cipherText)
        {
            return DecryptToBytes(Convert.FromBase64String(cipherText));
        }

        public byte[] DecryptToBytes(byte[] cipherTextBytes)
        {
            int decryptedByteCount = 0;
            int saltLen = 0;

            var memoryStream = new MemoryStream(cipherTextBytes);

            byte[] decryptedBytes = new byte[cipherTextBytes.Length];

            lock (this)
            {
                var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);

                decryptedByteCount = cryptoStream.Read(decryptedBytes, 0, decryptedBytes.Length);

                memoryStream.Close();
                cryptoStream.Close();
            }

            if (maxSaltLen > 0 && maxSaltLen >= minSaltLen)
            {
                saltLen = decryptedBytes[0] & 0x03 | decryptedBytes[1] & 0x0c | decryptedBytes[2] & 0x30 | decryptedBytes[3] & 0xc0;
            }

            byte[] plainTextBytes = new byte[decryptedByteCount - saltLen];

            Array.Copy(decryptedBytes, saltLen, plainTextBytes, 0, decryptedByteCount - saltLen);

            return plainTextBytes;
        }

        private byte[] AddSalt(byte[] plainTextBytes)
        {
            if (maxSaltLen == 0 || maxSaltLen < minSaltLen)
            {
                return plainTextBytes;
            }

            byte[] saltBytes = GenerateSalt();

            byte[] plainTextBytesWithSalt = new byte[plainTextBytes.Length + saltBytes.Length];

            Array.Copy(saltBytes, plainTextBytesWithSalt, saltBytes.Length);

            Array.Copy(plainTextBytes, 0, plainTextBytesWithSalt, saltBytes.Length, plainTextBytes.Length);

            return plainTextBytesWithSalt;
        }

        private byte[] GenerateSalt()
        {
            int saltLen;

            if (minSaltLen == maxSaltLen)
            {
                saltLen = minSaltLen;
            }
            else
            {
                saltLen = GenerateRandomNumber(minSaltLen, maxSaltLen);
            }

            byte[] salt = new byte[saltLen];

            var rng = new RNGCryptoServiceProvider();

            rng.GetNonZeroBytes(salt);

            salt[0] = (byte)(salt[0] & 0xfc | saltLen & 0x03);
            salt[1] = (byte)(salt[1] & 0xf3 | saltLen & 0x0c);
            salt[2] = (byte)(salt[2] & 0xcf | saltLen & 0x30);
            salt[3] = (byte)(salt[3] & 0x3f | saltLen & 0xc0);

            return salt;
        }

        private int GenerateRandomNumber(int minValue, int maxValue)
        {
            byte[] randomBytes = new byte[4];

            var rng = new RNGCryptoServiceProvider();

            rng.GetBytes(randomBytes);

            int seed = (randomBytes[0] & 0x7f) << 24 | randomBytes[1] << 16 | randomBytes[2] << 8 | randomBytes[3];

            var random = new Random(seed);

            return random.Next(minValue, maxValue + 1);
        }
    }
}
