using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Player;

public class RegisterScreenUseCase
{
    private readonly IScreenRepository _screenRepository;
    private readonly ILogger<RegisterScreenUseCase> _logger;

    public RegisterScreenUseCase(
        IScreenRepository screenRepository,
        ILogger<RegisterScreenUseCase> logger)
    {
        _screenRepository = screenRepository;
        _logger = logger;
    }

    public async Task<RegisterResponseDto> ExecuteAsync(string screenKey, string? deviceId)
    {
        var screen = await _screenRepository.GetByScreenKeyAsync(screenKey);

        if (screen is null)
        {
            var generatedDeviceId = GenerateDeviceId(deviceId);

            var newScreen = new Screen
            {
                Name = screenKey,
                ScreenKey = screenKey,
                DeviceId = generatedDeviceId,
                IsApproved = false,
                IsOnline = false,
                CreatedAt = DateTime.UtcNow
            };

            await _screenRepository.AddAsync(newScreen);

            _logger.LogInformation(
                "Screen registered with key {ScreenKey}, device {DeviceId}",
                screenKey,
                generatedDeviceId);

            return new RegisterResponseDto(
                generatedDeviceId,
                false,
                "Pantalla registrada. Pendiente de aprobación.");
        }

        return new RegisterResponseDto(
            screen.DeviceId,
            screen.IsApproved,
            screen.IsApproved
                ? "Pantalla activa."
                : "Pendiente de aprobación.");
    }

    private static string GenerateDeviceId(string? deviceId)
    {
        if (!string.IsNullOrWhiteSpace(deviceId))
            return deviceId;

        return $"DEVICE-{Guid.NewGuid():N}"[..4].ToUpperInvariant();
    }
}
