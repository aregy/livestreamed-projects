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
public class FileController : Controller
{
    private readonly IWebHostEnvironment _HostingEnvironment;
    FileUrlStorageService _FileUrlStorageService;
    public FileController(IWebHostEnvironment hostingEnvironment, FileUrlStorageService fileUrlStorageService)
    {
        _HostingEnvironment = hostingEnvironment;
        _FileUrlStorageService = fileUrlStorageService;
        //Path.Combine(Directory.GetCurrentDirectory, "wwwroot/uploads")
        var directoryInfo = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads").ToString());
        if (!directoryInfo.Exists) throw new FileNotFoundException();
        foreach (var file in directoryInfo.GetFiles("*.pdf"))
        {
            _FileUrlStorageService.Add(new FileTag() { Id = new Guid(), LastWriteTime = file.LastWriteTime, Name = file.Name });
        }
    }
    [HttpGet("{id}")]
    [DisableRequestSizeLimit]
    public FileStreamResult GetFile(Guid id)
    {
        try
        {
            var p = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/2004-accord.pdf");
            var st = System.IO.File.OpenRead(p.ToString());
            return new FileStreamResult(st, "application/pdf");
        }
        catch (Exception ex)
        {
            throw new Exception("GetFile() exceiption thrown");
        }
    }
    [HttpGet]
    [Route("Last")]
    [DisableRequestSizeLimit]
    public FileStreamResult Last()
    {
        try
        {
            
            var p = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            DirectoryInfo directoryInfo = new DirectoryInfo(p);
            if (!directoryInfo.Exists) throw new Exception();
            var files = directoryInfo.GetFiles("*.pdf").OrderBy(f => f.CreationTime).ToList();
            var st = System.IO.File.OpenRead(files.Last().FullName);
            return new FileStreamResult(st, "application/pdf");
        }
        catch (Exception ex)
        {
            throw new Exception("GetFile() exceiption thrown");
        }
    }
    [HttpGet]
    [Route("List")]
    [DisableRequestSizeLimit]
    public ActionResult<List<FileTag>> List()
    {
        try
        {

            //var p = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            //DirectoryInfo directoryInfo = new DirectoryInfo(p);
            //if (!directoryInfo.Exists) throw new Exception();
            //var fileTags = directoryInfo.GetFiles("*.pdf").OrderBy(f => f.CreationTime)
            //        .Select(f => new FileTag{ Name = f.Name, LastWriteTime=f.LastWriteTime})
            //        .ToList();
            var fileTags = _FileUrlStorageService.List();
            //var st = System.IO.File.OpenRead(files.Last().FullName);
            //return new FileStreamResult(st, "application/pdf");
            return this.Ok(fileTags);
        }
        catch (Exception ex)
        {
            throw new Exception("List() exception thrown");
        }
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
                    _FileUrlStorageService.Add(new FileTag() { Id = Guid.Parse(metadataObject.FileGuid), Name = @"uploads\" + metadataObject.FileName, LastWriteTime = DateTime.Now });

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