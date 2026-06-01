using System.Security.Claims;
using Application.Playlists;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/playlists")]
[Authorize]
public class PlaylistsController : ControllerBase
{
    private readonly GetPlaylistsUseCase _getPlaylistsUseCase;
    private readonly GetPlaylistDetailUseCase _getPlaylistDetailUseCase;
    private readonly CreatePlaylistUseCase _createPlaylistUseCase;
    private readonly UpdatePlaylistUseCase _updatePlaylistUseCase;
    private readonly DeletePlaylistUseCase _deletePlaylistUseCase;
    private readonly ReorderPlaylistUseCase _reorderPlaylistUseCase;
    private readonly AddMediaToPlaylistUseCase _addMediaToPlaylistUseCase;
    private readonly RemoveMediaFromPlaylistUseCase _removeMediaFromPlaylistUseCase;

    public PlaylistsController(
        GetPlaylistsUseCase getPlaylistsUseCase,
        GetPlaylistDetailUseCase getPlaylistDetailUseCase,
        CreatePlaylistUseCase createPlaylistUseCase,
        UpdatePlaylistUseCase updatePlaylistUseCase,
        DeletePlaylistUseCase deletePlaylistUseCase,
        ReorderPlaylistUseCase reorderPlaylistUseCase,
        AddMediaToPlaylistUseCase addMediaToPlaylistUseCase,
        RemoveMediaFromPlaylistUseCase removeMediaFromPlaylistUseCase)
    {
        _getPlaylistsUseCase = getPlaylistsUseCase;
        _getPlaylistDetailUseCase = getPlaylistDetailUseCase;
        _createPlaylistUseCase = createPlaylistUseCase;
        _updatePlaylistUseCase = updatePlaylistUseCase;
        _deletePlaylistUseCase = deletePlaylistUseCase;
        _reorderPlaylistUseCase = reorderPlaylistUseCase;
        _addMediaToPlaylistUseCase = addMediaToPlaylistUseCase;
        _removeMediaFromPlaylistUseCase = removeMediaFromPlaylistUseCase;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _getPlaylistsUseCase.ExecuteAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id) =>
        Ok(await _getPlaylistDetailUseCase.ExecuteAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePlaylistRequest request)
    {
        var adminUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var playlist = await _createPlaylistUseCase.ExecuteAsync(request.Name, adminUserId);
        return CreatedAtAction(nameof(GetById), new { id = playlist.Id }, playlist);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePlaylistRequest request)
    {
        var adminUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _updatePlaylistUseCase.ExecuteAsync(id, request.Name, adminUserId));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var adminUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _deletePlaylistUseCase.ExecuteAsync(id, adminUserId);
        return NoContent();
    }

    [HttpPut("{id:int}/reorder")]
    public async Task<IActionResult> Reorder(int id, [FromBody] IEnumerable<ReorderItemDto> items)
    {
        await _reorderPlaylistUseCase.ExecuteAsync(id, items);
        return NoContent();
    }

    [HttpPost("{id:int}/media")]
    public async Task<IActionResult> AddMedia(int id, [FromBody] AddMediaToPlaylistRequest request)
    {
        await _addMediaToPlaylistUseCase.ExecuteAsync(
            id,
            request.MediaItemId,
            request.SortOrder,
            request.OverrideDuration);

        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpDelete("{id:int}/media/{mediaId:int}")]
    public async Task<IActionResult> RemoveMedia(int id, int mediaId)
    {
        await _removeMediaFromPlaylistUseCase.ExecuteAsync(id, mediaId);
        return NoContent();
    }

    public record CreatePlaylistRequest(string Name);
    public record UpdatePlaylistRequest(string Name);
    public record AddMediaToPlaylistRequest(int MediaItemId, int SortOrder, int? OverrideDuration);
}
