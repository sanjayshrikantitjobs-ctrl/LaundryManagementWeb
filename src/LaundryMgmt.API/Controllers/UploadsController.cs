using LaundryMgmt.Application.Common.Constants;
using LaundryMgmt.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaundryMgmt.API.Controllers;

/// <summary>Image upload for garment/service/category/promotion photos, stored in
/// Azure Blob Storage (see IImageStorageService) — not local disk, which doesn't
/// survive a redeploy or scale across instances.</summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = AppRoles.ImageUploadRoles)]
public class UploadsController : ControllerBase
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    private readonly IImageStorageService _imageStorage;

    public UploadsController(IImageStorageService imageStorage) => _imageStorage = imageStorage;

    /// <summary>Uploads an image (garment/service photo) and returns its public URL.</summary>
    [HttpPost("images")]
    [RequestSizeLimit(MaxFileSizeBytes)]
    [ProducesResponseType(typeof(UploadedImageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UploadedImageDto>> UploadImage(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest("No file uploaded.");

        if (file.Length > MaxFileSizeBytes)
            return BadRequest("File is too large (max 5 MB).");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            return BadRequest($"Unsupported file type. Allowed: {string.Join(", ", AllowedExtensions)}");

        var fileName = $"{Guid.NewGuid()}{extension}";

        await using var stream = file.OpenReadStream();
        var url = await _imageStorage.UploadAsync(stream, fileName, file.ContentType, cancellationToken);

        return Ok(new UploadedImageDto(url));
    }
}

public record UploadedImageDto(string Url);
