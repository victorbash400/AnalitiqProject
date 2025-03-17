using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

[Route("api/[controller]")]
[ApiController]
public class FileUploadController : ControllerBase
{
    private readonly BlobStorageService _blobService;
    private readonly ILogger<FileUploadController> _logger;

    public FileUploadController(BlobStorageService blobService, ILogger<FileUploadController> logger)
    {
        _blobService = blobService;
        _logger = logger;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(
        [FromForm] IFormFile file,
        [FromForm] string tenantId,
        [FromForm] string batchId,
        [FromForm] string productName) // Added productName parameter
    {
        _logger.LogInformation("Received request to upload file.");

        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("File is empty.");
            return BadRequest("File is empty");
        }

        _logger.LogInformation($"Uploading file: {file.FileName}, Tenant: {tenantId}, Batch: {batchId}, Product: {productName}");

        using var stream = file.OpenReadStream();
        var fileUrl = await _blobService.UploadFileAsync(stream, file.FileName, tenantId, batchId, productName); // Pass productName

        _logger.LogInformation($"Upload successful: {fileUrl}");

        return Ok(new { Message = "Upload successful", FileUrl = fileUrl });
    }
}