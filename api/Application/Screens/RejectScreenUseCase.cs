using Domain.Exceptions;
using Domain.Interfaces;

namespace Application.Screens;

public class RejectScreenUseCase
{
    private readonly IScreenRepository _screenRepository;

    public RejectScreenUseCase(IScreenRepository screenRepository)
    {
        _screenRepository = screenRepository;
    }

    public async Task ExecuteAsync(int id)
    {
        var screen = await _screenRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Screen with id {id} was not found.");

        await _screenRepository.DeleteAsync(screen.Id);
    }
}
