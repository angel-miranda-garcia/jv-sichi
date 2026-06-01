using Domain.Exceptions;
using Domain.Interfaces;

namespace Application.Screens;

public class DeleteScreenUseCase
{
    private readonly IScreenRepository _screenRepository;

    public DeleteScreenUseCase(IScreenRepository screenRepository)
    {
        _screenRepository = screenRepository;
    }

    public async Task ExecuteAsync(int id)
    {
        var screen = await _screenRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Screen with id {id} was not found.");

        if (screen.CurrentPlaylistId is not null)
            throw new ConflictException("La pantalla tiene una playlist asignada");

        await _screenRepository.DeleteAsync(id);
    }
}
