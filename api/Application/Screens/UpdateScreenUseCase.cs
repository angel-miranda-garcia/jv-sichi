using Domain.Exceptions;
using Domain.Interfaces;

namespace Application.Screens;

public class UpdateScreenUseCase
{
    private readonly IScreenRepository _screenRepository;

    public UpdateScreenUseCase(IScreenRepository screenRepository)
    {
        _screenRepository = screenRepository;
    }

    public async Task<ScreenDto> ExecuteAsync(int id, string name)
    {
        var screen = await _screenRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Screen with id {id} was not found.");

        screen.Name = name;
        await _screenRepository.UpdateAsync(screen);

        var updated = await _screenRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Screen with id {id} was not found.");

        return ScreenMapper.ToDto(updated);
    }
}
