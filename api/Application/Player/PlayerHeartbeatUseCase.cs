using Domain.Interfaces;

namespace Application.Player;

public class PlayerHeartbeatUseCase
{
    private readonly IScreenRepository _screenRepository;

    public PlayerHeartbeatUseCase(IScreenRepository screenRepository)
    {
        _screenRepository = screenRepository;
    }

    public async Task ExecuteAsync(string screenKey)
    {
        var screen = await _screenRepository.GetByScreenKeyAsync(screenKey);
        if (screen is null)
            return;

        screen.LastPing = DateTime.UtcNow;
        screen.IsOnline = true;
        await _screenRepository.UpdateAsync(screen);
    }
}
