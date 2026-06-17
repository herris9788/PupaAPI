using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Pupa.Helpers
{
    public class AESEnDecryption
    {
        public byte[] Encrypt(byte[] bytes, string keyStr, string ivStr)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();
            byte[] ivBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(ivStr));
            byte[] keyBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(keyStr));

            return Encrypt(bytes, keyBytes, ivBytes);
        }

        private byte[] Encrypt(byte[] bytes, byte[] keyBytes, byte[] ivBytes)
        {
            //RijndaelManaged aes = new RijndaelManaged();
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();

            aes.IV = ivBytes;
            aes.Key = keyBytes;
            //aes.Mode = CipherMode.CBC;
            //aes.Padding = PaddingMode.PKCS7;

            ICryptoTransform transform = aes.CreateEncryptor();
            return transform.TransformFinalBlock(bytes, 0, bytes.Length);
        }

        public byte[] Decrypt(byte[] bytes, string keyStr, string ivStr)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();
            byte[] ivBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(ivStr));
            byte[] keyBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(keyStr));

            return Decrypt(bytes, keyBytes, ivBytes);
        }

        private byte[] Decrypt(byte[] bytes, byte[] keyBytes, byte[] ivBytes)
        {
            //RijndaelManaged aes = new RijndaelManaged();
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();

            aes.IV = ivBytes;
            aes.Key = keyBytes;
            //aes.Mode = CipherMode.CBC;
            //aes.Padding = PaddingMode.PKCS7;

            ICryptoTransform transform = aes.CreateDecryptor();
            return transform.TransformFinalBlock(bytes, 0, bytes.Length);
        }

        public string EncryptStrAndToBase64(string enStr, string keyStr, string ivStr)
        {
            byte[] bytes = Encrypt(Encoding.UTF8.GetBytes(enStr), keyStr, ivStr);
            return Convert.ToBase64String(bytes);
        }

        public string DecryptStrAndFromBase64(string deStr, string keyStr, string ivStr)
        {
            byte[] bytes = Decrypt(Convert.FromBase64String(deStr), keyStr, ivStr);
            return Encoding.UTF8.GetString(bytes);
        }

        public void BinarySaveObjectWithAes<T>(T theObject, string fileName, string ivStr, string keyStr)
        {
            Stream streamWrite = File.Create(fileName);
            MemoryStream memoryStreamWrite = new MemoryStream();
            BinaryFormatter binaryWrite = new BinaryFormatter();
            binaryWrite.Serialize(memoryStreamWrite, theObject);
            memoryStreamWrite = new MemoryStream(Encrypt(memoryStreamWrite.ToArray(), keyStr, ivStr));
            memoryStreamWrite.WriteTo(streamWrite);
            memoryStreamWrite.Close();
            streamWrite.Close();
        }

        public T BinaryReadObjectWithAes<T>(string fileName, string ivStr, string keyStr)
        {
            Stream streamRead = File.OpenRead(fileName);
            MemoryStream memoryStreamRead = new MemoryStream();
            BinaryFormatter binaryRead = new BinaryFormatter();
            streamRead.CopyTo(memoryStreamRead);
            memoryStreamRead = new MemoryStream(Decrypt(memoryStreamRead.ToArray(), keyStr, ivStr));
            T theObject = (T)binaryRead.Deserialize(memoryStreamRead);
            streamRead.Close();
            memoryStreamRead.Close();
            return theObject;
        }
    }
}
