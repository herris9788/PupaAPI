using Pupa.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Pupa.BusinessObjects;

namespace Pupa.Controllers
{
    [ApiController]
    [Route("beesuite/api/[controller]")]
    public class MediaController : ControllerBase
    {
        private readonly IFtpService _ftpService;
        private readonly BeesuiteDbContext _db;

        public MediaController(IFtpService ftpService, BeesuiteDbContext db)
        {
            _ftpService = ftpService;
            _db = db;
        }

        [HttpGet("partbooks")]
        public async Task<IActionResult> PartBookList([FromQuery] string Vessel = "")
        {
            try
            {
                if (string.IsNullOrEmpty(Vessel))
                {
                    var _PartBooks = _db.PartBook.AsQueryable();
                    return Ok(_PartBooks); 
                }

                var PartBooks = _db.PartBook.Where(x => x.VesselName == Vessel).AsQueryable();
                return Ok(PartBooks);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("list")]
        public async Task<IActionResult> List([FromQuery] string path = "/")
        {
            try
            {
                var items = await _ftpService.ListItemsAsync(path);
                return Ok(items);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file, [FromQuery] string remotePath)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            try
            {
                using var stream = file.OpenReadStream();
                // Combine remotePath and filename if remotePath is a directory
                string fullRemotePath = remotePath.EndsWith("/") ? remotePath + file.FileName : remotePath;
                
                var success = await _ftpService.UploadFileAsync(stream, fullRemotePath);
                if (success)
                    return Ok(new { message = "Upload successful", path = fullRemotePath });
                
                return StatusCode(500, "Upload failed.");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("download")]
        public async Task<IActionResult> Download([FromQuery] string path)
        {
            try
            {
                var stream = await _ftpService.DownloadFileAsync(path);
                var fileName = Path.GetFileName(path);
                return File(stream, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> Delete([FromQuery] string path)
        {
            try
            {
                var success = await _ftpService.DeleteFileAsync(path);
                if (success)
                    return Ok(new { message = "Delete successful" });
                
                return BadRequest("Delete failed.");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("rename")]
        public async Task<IActionResult> Rename([FromQuery] string oldPath, [FromQuery] string newPath)
        {
            try
            {
                var success = await _ftpService.RenameAsync(oldPath, newPath);
                if (success)
                    return Ok(new { message = "Rename successful" });
                
                return BadRequest("Rename failed.");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("mkdir")]
        public async Task<IActionResult> CreateDirectory([FromQuery] string path)
        {
            try
            {
                var success = await _ftpService.CreateDirectoryAsync(path);
                if (success)
                    return Ok(new { message = "Directory created" });
                
                return BadRequest("Creation failed.");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("rmdir")]
        public async Task<IActionResult> DeleteDirectory([FromQuery] string path)
        {
            try
            {
                var success = await _ftpService.DeleteDirectoryAsync(path);
                if (success)
                    return Ok(new { message = "Directory deleted" });
                
                return BadRequest("Deletion failed.");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
