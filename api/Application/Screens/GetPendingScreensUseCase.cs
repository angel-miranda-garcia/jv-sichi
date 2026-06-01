using Domain.Interfaces;

namespace Application.Screens;

public class GetPendingScreensUseCase
{
    private readonly IScreenRepository _screenRepository;

    public GetPendingScreensUseCase(IScreenRepository screenRepository)
    {
        _screenRepository = screenRepository;
    }

    public async Task<IEnumerable<PendingScreenDto>> ExecuteAsync()
    {
        var screens = await _screenRepository.GetPendingAsync();
        return screens.Select(ScreenMapper.ToPendingDto);
    }
}
