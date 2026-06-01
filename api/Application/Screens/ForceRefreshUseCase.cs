using Domain.Exceptions;
using Domain.Interfaces;

namespace Application.Screens;

public class ForceRefreshUseCase
{
    private readonly IScreenRepository _screenRepository;
    private readonly IRealtimeService _realtimeService;

    public ForceRefreshUseCase(
        IScreenRepository screenRepository,
        IRealtimeService realtimeService)
    {
        _screenRepository = screenRepository;
        _realtimeService = realtimeService;
    }

    public async Task ExecuteAsync(int screenId)
    {
        var screen = await _screenRepository.GetByIdAsync(screenId)
            ?? throw new NotFoundException($"Screen with id {screenId} was not found.");

        await _realtimeService.SendForceRefreshAsync(screen.ScreenKey);
    }
}
