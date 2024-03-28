using devexBlazor.Data;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace devexBlazor.Controllers;
public class ChunkMetadata
{
    public int Index { get; set; }
    public int TotalCount { get; set; }
    public int FileSize { get; set; }
    public string? FileName { get; set; }
    public string? FileType { get; set; }
    public string? FileGuid { get; set; }
}
[Route("api/[controller]")]
public class UploadController : Controller
{
    private readonly IWebHostEnvironment _HostingEnvironment;
    FileUrlStorageService _FileUrlStorageService;
    public UploadController(IWebHostEnvironment hostingEnvironment, FileUrlStorageService fileUrlStorageService)
    {
        _HostingEnvironment = hostingEnvironment;
        _FileUrlStorageService = fileUrlStorageService;
    }

    [HttpPost]
    [Route("UploadFile")]
    [DisableRequestSizeLimit]
    public ActionResult UploadFile(IFormFile imageUpload, string chunkMetadata)
    {
        var tempPath = Path.Combine(_HostingEnvironment.WebRootPath, "uploads");
        // Removes temporary files
        RemoveTempFilesAfterDelay(tempPath, new TimeSpan(0, 5, 0));

        try
        {
            if (!string.IsNullOrEmpty(chunkMetadata))
            {
                var metadataObject = JsonConvert.DeserializeObject<ChunkMetadata>(chunkMetadata);
                var tempFilePath = Path.Combine(tempPath, metadataObject.FileGuid + ".tmp");
                if (!Directory.Exists(tempPath))
                    Directory.CreateDirectory(tempPath);

                AppendContentToFile(tempFilePath, imageUpload);

                if (metadataObject.Index == (metadataObject.TotalCount - 1))
                {
                    ProcessUploadedFile(tempFilePath, metadataObject.FileName);
                    _FileUrlStorageService.Add(Guid.Parse(metadataObject.FileGuid), @"uploads\" + metadataObject.FileName);
                }
            }
        }
        catch (Exception ex)
        {
            return BadRequest();
        }
        return Ok();
    }
    void RemoveTempFilesAfterDelay(string path, TimeSpan delay)
    {
        var dir = new DirectoryInfo(path);
        if (dir.Exists)
            foreach (var file in dir.GetFiles("*.tmp").Where(f => f.LastWriteTimeUtc.Add(delay) < DateTime.UtcNow))
                file.Delete();
    }
    void ProcessUploadedFile(string tempFilePath, string fileName)
    {
        var path = Path.Combine(_HostingEnvironment.WebRootPath, "uploads");
        var imagePath = Path.Combine(path, fileName);
        if (System.IO.File.Exists(imagePath))
            System.IO.File.Delete(imagePath);
        System.IO.File.Copy(tempFilePath, imagePath);
    }
    void AppendContentToFile(string path, IFormFile content)
    {
        using (var stream = new FileStream(path, FileMode.Append, FileAccess.Write))
        {
            content.CopyTo(stream);
        }
    }
}

