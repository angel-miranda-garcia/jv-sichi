using Application.Player;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/player")]
public class PlayerController : ControllerBase
{
    private readonly RegisterScreenUseCase _registerScreenUseCase;
    private readonly SyncPlaylistUseCase _syncPlaylistUseCase;
    private readonly GetPlayerVersionUseCase _getPlayerVersionUseCase;
    private readonly PlayerHeartbeatUseCase _playerHeartbeatUseCase;

    public PlayerController(
        RegisterScreenUseCase registerScreenUseCase,
        SyncPlaylistUseCase syncPlaylistUseCase,
        GetPlayerVersionUseCase getPlayerVersionUseCase,
        PlayerHeartbeatUseCase playerHeartbeatUseCase)
    {
        _registerScreenUseCase = registerScreenUseCase;
        _syncPlaylistUseCase = syncPlaylistUseCase;
        _getPlayerVersionUseCase = getPlayerVersionUseCase;
        _playerHeartbeatUseCase = playerHeartbeatUseCase;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto) =>
        Ok(await _registerScreenUseCase.ExecuteAsync(dto.ScreenKey, dto.DeviceId));

    [HttpGet("playlist")]
    public async Task<IActionResult> GetPlaylist([FromQuery] string screen) =>
        Ok(await _syncPlaylistUseCase.ExecuteAsync(screen));

    [HttpGet("version")]
    public async Task<IActionResult> GetVersion([FromQuery] string screen) =>
        Ok(await _getPlayerVersionUseCase.ExecuteAsync(screen));

    [HttpPost("heartbeat")]
    public async Task<IActionResult> Heartbeat([FromBody] HeartbeatRequestDto dto)
    {
        await _playerHeartbeatUseCase.ExecuteAsync(dto.ScreenKey);
        return NoContent();
    }
}
