using Microsoft.AspNetCore.Mvc;
using QalatAldhaman.Store.Api.DTOs.Uploads;

namespace QalatAldhaman.Store.Api.Controllers;

[ApiController]
[Route("api/uploads")]
public class UploadsController : ControllerBase
{
    private static readonly string[] AllowedFolders = ["categories", "products", "contracts"];
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxImageSizeBytes = 10 * 1024 * 1024;
    private const long MaxPdfSizeBytes = 20 * 1024 * 1024;

    private readonly IWebHostEnvironment _environment;

    public UploadsController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    [HttpPost]
    [RequestSizeLimit(MaxPdfSizeBytes)]
    public async Task<ActionResult<UploadResponseDto>> Upload(IFormFile file, [FromQuery] string folder = "products")
    {
        if (!AllowedFolders.Contains(folder))
        {
            return BadRequest(new { message = $"folder غير صحيح. القيم المسموحة: {string.Join(", ", AllowedFolders)}" });
        }

        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "لم يتم إرسال أي ملف" });
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var isContract = folder == "contracts";

        if (isContract)
        {
            if (extension != ".pdf")
            {
                return BadRequest(new { message = "يُسمح فقط برفع ملفات PDF لهذا النوع" });
            }

            if (file.Length > MaxPdfSizeBytes)
            {
                return BadRequest(new { message = "حجم الملف يتجاوز الحد الأقصى المسموح (20MB)" });
            }
        }
        else
        {
            if (!AllowedImageExtensions.Contains(extension))
            {
                return BadRequest(new { message = "يُسمح فقط برفع صور بصيغة jpg أو png أو webp" });
            }

            if (file.Length > MaxImageSizeBytes)
            {
                return BadRequest(new { message = "حجم الملف يتجاوز الحد الأقصى المسموح (10MB)" });
            }
        }

        var webRootPath = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var targetDirectory = Path.Combine(webRootPath, "uploads", folder);
        Directory.CreateDirectory(targetDirectory);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(targetDirectory, fileName);

        await using (var stream = System.IO.File.Create(fullPath))
        {
            await file.CopyToAsync(stream);
        }

        return Ok(new UploadResponseDto { Url = $"/uploads/{folder}/{fileName}" });
    }
}
