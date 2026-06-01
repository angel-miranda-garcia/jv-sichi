using Domain.Interfaces;

namespace Application.Screens;

public class GetApprovedScreensUseCase
{
    private readonly IScreenRepository _screenRepository;

    public GetApprovedScreensUseCase(IScreenRepository screenRepository)
    {
        _screenRepository = screenRepository;
    }

    public async Task<IEnumerable<ScreenDto>> ExecuteAsync()
    {
        var screens = await _screenRepository.GetApprovedAsync();
        return screens.Select(ScreenMapper.ToDto);
    }
}
