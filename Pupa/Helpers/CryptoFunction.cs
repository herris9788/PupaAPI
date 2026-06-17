namespace Pupa.Helpers
{
    /// <summary>
    /// Custom Build Encryption Function
    /// </summary>

    public class CryptoFunction
    {
        public static string Encrypt(string plainText)
        {
            var crypto = new CryptoExtension("tC88Tt5uWx1s5788cUeDIkq0QZudbwUkk5HxL1FcB5FwdXySOnFU/6kSBsIVhfe7m4v8zPySJOxYx10$UXSLPg==", "a8uAtChAim5778Lo", 4, 8, 128, "SHA512");

            return crypto.Encrypt(plainText);
        }

        public static string Decrypt(string plainText)
        {
            var crypto = new CryptoExtension("tC88Tt5uWx1s5788cUeDIkq0QZudbwUkk5HxL1FcB5FwdXySOnFU/6kSBsIVhfe7m4v8zPySJOxYx10$UXSLPg==", "a8uAtChAim5778Lo", 4, 8, 128, "SHA512");

            return crypto.Decrypt(plainText);
        }
    }
}
