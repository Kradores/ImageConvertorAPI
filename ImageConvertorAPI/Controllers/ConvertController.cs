using ImageConvertorAPI.Models;
using ImageConvertorAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ImageConvertorAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConvertController : ControllerBase
{
    private readonly IImageConversionService _service;

    public ConvertController(IImageConversionService service)
    {
        _service = service;
    }

    /// <summary>
    /// Converts uploaded images into multiple WebP sizes and returns a ZIP archive.
    /// </summary>
    /// <remarks>
    /// Max 100 files.
    /// Max 50MB per file.
    /// Supported formats: jpg, jpeg, png, webp.
    /// </remarks>
    /// <response code="200">Returns ZIP file</response>
    /// <response code="400">Invalid request</response>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [Produces("application/zip")]
    [RequestSizeLimit(long.MaxValue)]
    public async Task<IActionResult> Convert(
    [FromForm] ConvertRequest request,
    CancellationToken cancellationToken)
    {
        if (request.Files.Count == 0)
            return BadRequest("No files uploaded.");

        if (request.Files.Count > 100)
            return BadRequest("Maximum 100 files allowed.");

        var zipBytes = await _service.ConvertAsync(request, cancellationToken);

        return File(zipBytes, "application/zip", "converted.zip");
    }
}
