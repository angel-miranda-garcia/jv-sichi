using Domain.Exceptions;
using Domain.Interfaces;

namespace Application.Screens;

public class ApproveScreenUseCase
{
    private readonly IScreenRepository _screenRepository;
    private readonly IAuditService _auditService;

    public ApproveScreenUseCase(IScreenRepository screenRepository, IAuditService auditService)
    {
        _screenRepository = screenRepository;
        _auditService = auditService;
    }

    public async Task<ScreenDto> ExecuteAsync(int id, int adminUserId)
    {
        var screen = await _screenRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Screen with id {id} was not found.");

        screen.IsApproved = true;
        await _screenRepository.UpdateAsync(screen);

        await _auditService.LogAsync(
            "ScreenApproved",
            adminUserId,
            id,
            $"Screen {screen.Name} approved");

        var updated = await _screenRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Screen with id {id} was not found.");

        return ScreenMapper.ToDto(updated);
    }
}
