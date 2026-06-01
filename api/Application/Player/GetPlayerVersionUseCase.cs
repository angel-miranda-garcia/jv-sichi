using Domain.Exceptions;
using Domain.Interfaces;

namespace Application.Player;

public class GetPlayerVersionUseCase
{
    private readonly IScreenRepository _screenRepository;

    public GetPlayerVersionUseCase(IScreenRepository screenRepository)
    {
        _screenRepository = screenRepository;
    }

    public async Task<PlayerVersionDto> ExecuteAsync(string screenKey)
    {
        var screen = await _screenRepository.GetByScreenKeyAsync(screenKey)
            ?? throw new NotFoundException($"Screen with key '{screenKey}' was not found.");

        if (!screen.IsApproved)
            return new PlayerVersionDto(0, null);

        return new PlayerVersionDto(
            screen.CurrentPlaylist?.Version ?? 0,
            screen.CurrentPlaylistId);
    }
}
