using System.Security.Claims;
using Application.Media;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/media")]
[Authorize]
public class MediaController : ControllerBase
{
    private readonly IMediaRepository _mediaRepository;
    private readonly UploadMediaUseCase _uploadMediaUseCase;
    private readonly GetMediaUsageUseCase _getMediaUsageUseCase;
    private readonly DeleteMediaUseCase _deleteMediaUseCase;

    public MediaController(
        IMediaRepository mediaRepository,
        UploadMediaUseCase uploadMediaUseCase,
        GetMediaUsageUseCase getMediaUsageUseCase,
        DeleteMediaUseCase deleteMediaUseCase)
    {
        _mediaRepository = mediaRepository;
        _uploadMediaUseCase = uploadMediaUseCase;
        _getMediaUsageUseCase = getMediaUsageUseCase;
        _deleteMediaUseCase = deleteMediaUseCase;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _mediaRepository.GetAllAsync();
        return Ok(items.Select(MediaMapper.ToDto));
    }

    [HttpPost("upload")]
    [RequestSizeLimit(52_428_800)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        var adminUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var mediaItem = await _uploadMediaUseCase.ExecuteAsync(file, adminUserId);
        return CreatedAtAction(nameof(GetUsage), new { id = mediaItem.Id }, mediaItem);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var adminUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _deleteMediaUseCase.ExecuteAsync(id, adminUserId);
        return NoContent();
    }

    [HttpGet("{id:int}/usage")]
    public async Task<IActionResult> GetUsage(int id) =>
        Ok(await _getMediaUsageUseCase.ExecuteAsync(id));
}
