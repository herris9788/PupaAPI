using FluentFTP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Pupa.Services
{
    public interface IFtpService
    {
        Task<List<FtpListItemDto>> ListItemsAsync(string path);
        Task<bool> UploadFileAsync(Stream fileStream, string remotePath);
        Task<Stream> DownloadFileAsync(string remotePath);
        Task<bool> DeleteFileAsync(string remotePath);
        Task<bool> RenameAsync(string oldPath, string newPath);
        Task<bool> CreateDirectoryAsync(string path);
        Task<bool> DeleteDirectoryAsync(string path);
    }

    public class FtpListItemDto
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public FtpObjectType Type { get; set; }
        public long Size { get; set; }
        public DateTime Modified { get; set; }
    }
}
