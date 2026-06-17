using FluentFTP;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Pupa.Services
{
    public class FtpService : IFtpService
    {
        private readonly IConfiguration _configuration;
        private readonly string _host;
        private readonly int _port;
        private readonly string _user;
        private readonly string _pass;

        public FtpService(IConfiguration configuration)
        {
            _configuration = configuration;
            _host = _configuration["FtpConfig:Host"];
            _port = int.Parse(_configuration["FtpConfig:Port"] ?? "21");
            _user = _configuration["FtpConfig:User"];
            _pass = _configuration["FtpConfig:Password"];
        }

        private AsyncFtpClient GetClient()
        {
            return new AsyncFtpClient(_host, _user, _pass, _port);
        }

        public async Task<List<FtpListItemDto>> ListItemsAsync(string path)
        {
            using var client = GetClient();
            await client.Connect();
            
            var items = await client.GetListing(path);
            
            return items.Select(item => new FtpListItemDto
            {
                Name = item.Name,
                FullPath = item.FullName,
                Type = item.Type,
                Size = item.Size,
                Modified = item.Modified
            }).ToList();
        }

        public async Task<bool> UploadFileAsync(Stream fileStream, string remotePath)
        {
            using var client = GetClient();
            await client.Connect();

            var status = await client.UploadStream(fileStream, remotePath, FtpRemoteExists.Overwrite, true);
            return status == FtpStatus.Success;
        }

        public async Task<Stream> DownloadFileAsync(string remotePath)
        {
            using var client = GetClient();
            await client.Connect();

            var ms = new MemoryStream();
            await client.DownloadStream(ms, remotePath);
            ms.Position = 0;
            return ms;
        }

        public async Task<bool> DeleteFileAsync(string remotePath)
        {
            using var client = GetClient();
            await client.Connect();

            await client.DeleteFile(remotePath);
            return true;
        }

        public async Task<bool> RenameAsync(string oldPath, string newPath)
        {
            using var client = GetClient();
            await client.Connect();

            await client.Rename(oldPath, newPath);
            return true;
        }

        public async Task<bool> CreateDirectoryAsync(string path)
        {
            using var client = GetClient();
            await client.Connect();

            await client.CreateDirectory(path);
            return true;
        }

        public async Task<bool> DeleteDirectoryAsync(string path)
        {
            using var client = GetClient();
            await client.Connect();

            await client.DeleteDirectory(path);
            return true;
        }
    }
}
