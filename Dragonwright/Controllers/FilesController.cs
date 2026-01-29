using Dragonwright.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dragonwright.Controllers;

/// <summary>
/// Controller for serving stored files.
/// </summary>
[ApiController]
[Route("files")]
public sealed class FilesController(FileStorageService fileStorageService) : ControllerBase
{
    /// <summary>
    /// Downloads a stored file by its ID.
    /// </summary>
    /// <param name="id">The ID of the stored file.</param>
    /// <returns>The file content with appropriate content type headers.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetFile(Guid id)
    {
        var storedFile = await fileStorageService.GetFileInfoAsync(id);
        if (storedFile == null)
        {
            return NotFound();
        }

        var stream = fileStorageService.OpenReadStream(storedFile);
        if (stream == null)
        {
            return NotFound();
        }

        return File(stream, storedFile.ContentType, storedFile.FileName);
    }
}
