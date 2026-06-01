using System.Security.Claims;
using Application.Playlists;
using Application.Screens;
using Domain.Exceptions;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/screens")]
[Authorize]
public class ScreensController : ControllerBase
{
    private readonly GetApprovedScreensUseCase _getApprovedScreensUseCase;
    private readonly GetPendingScreensUseCase _getPendingScreensUseCase;
    private readonly ApproveScreenUseCase _approveScreenUseCase;
    private readonly RejectScreenUseCase _rejectScreenUseCase;
    private readonly UpdateScreenUseCase _updateScreenUseCase;
    private readonly DeleteScreenUseCase _deleteScreenUseCase;
    private readonly IScreenRepository _screenRepository;
    private readonly AssignPlaylistToScreenUseCase _assignPlaylistToScreenUseCase;
    private readonly ForceRefreshUseCase _forceRefreshUseCase;

    public ScreensController(
        GetApprovedScreensUseCase getApprovedScreensUseCase,
        GetPendingScreensUseCase getPendingScreensUseCase,
        ApproveScreenUseCase approveScreenUseCase,
        RejectScreenUseCase rejectScreenUseCase,
        UpdateScreenUseCase updateScreenUseCase,
        DeleteScreenUseCase deleteScreenUseCase,
        IScreenRepository screenRepository,
        AssignPlaylistToScreenUseCase assignPlaylistToScreenUseCase,
        ForceRefreshUseCase forceRefreshUseCase)
    {
        _getApprovedScreensUseCase = getApprovedScreensUseCase;
        _getPendingScreensUseCase = getPendingScreensUseCase;
        _approveScreenUseCase = approveScreenUseCase;
        _rejectScreenUseCase = rejectScreenUseCase;
        _updateScreenUseCase = updateScreenUseCase;
        _deleteScreenUseCase = deleteScreenUseCase;
        _screenRepository = screenRepository;
        _assignPlaylistToScreenUseCase = assignPlaylistToScreenUseCase;
        _forceRefreshUseCase = forceRefreshUseCase;
    }

    [HttpGet]
    public async Task<IActionResult> GetApproved() =>
        Ok(await _getApprovedScreensUseCase.ExecuteAsync());

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending() =>
        Ok(await _getPendingScreensUseCase.ExecuteAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var screen = await _screenRepository.GetByIdAsync(id);
        if (screen is null)
            throw new NotFoundException($"Screen with id {id} was not found.");

        return Ok(ScreenMapper.ToDto(screen));
    }

    [HttpGet("{id:int}/status")]
    public async Task<IActionResult> GetStatus(int id)
    {
        var screen = await _screenRepository.GetByIdAsync(id);
        if (screen is null)
            throw new NotFoundException($"Screen with id {id} was not found.");

        return Ok(new
        {
            isOnline = screen.IsOnline,
            lastPing = screen.LastPing,
            currentPlaylistId = screen.CurrentPlaylistId
        });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateScreenRequest request) =>
        Ok(await _updateScreenUseCase.ExecuteAsync(id, request.Name));

    [HttpPut("{id:int}/playlist")]
    public async Task<IActionResult> AssignPlaylist(int id, [FromBody] AssignPlaylistDto dto)
    {
        var adminUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _assignPlaylistToScreenUseCase.ExecuteAsync(id, dto.PlaylistId, adminUserId);
        return Ok();
    }

    [HttpPost("{id:int}/force-refresh")]
    public async Task<IActionResult> ForceRefresh(int id)
    {
        await _forceRefreshUseCase.ExecuteAsync(id);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _deleteScreenUseCase.ExecuteAsync(id);
        return NoContent();
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var adminUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _approveScreenUseCase.ExecuteAsync(id, adminUserId));
    }

    [HttpDelete("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id)
    {
        await _rejectScreenUseCase.ExecuteAsync(id);
        return NoContent();
    }

    public record UpdateScreenRequest(string Name);
}
