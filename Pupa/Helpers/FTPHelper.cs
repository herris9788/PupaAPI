using System.Net;

namespace Pupa.Helpers
{
    public static class FTPHelper
    {
        public static string UploadFile(string ftpUrl, string userName, string password, Stream fileStream, string fileName)
        {
            try
            {
                string uri = $"{ftpUrl}/{fileName}";
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uri);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential(userName, password);

                // Get the length of the stream if possible
                long contentLength = fileStream.CanSeek ? fileStream.Length : -1;
                if (contentLength >= 0)
                {
                    request.ContentLength = contentLength;
                }

                using (Stream requestStream = request.GetRequestStream())
                {
                    fileStream.CopyTo(requestStream);
                }
                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    return $"Upload File Complete, status {response.StatusDescription}";
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
        public static byte[] GetFile(string ftpUrl, string userName, string password, string fileName)
        {
            try
            {
                string uri = $"{ftpUrl}/{fileName}";
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uri);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential(userName, password);
                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                using (Stream responseStream = response.GetResponseStream())
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    responseStream.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
            catch
            {
                return null;
            }
        }

    }
}
