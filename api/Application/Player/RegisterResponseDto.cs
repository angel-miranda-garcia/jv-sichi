namespace Application.Player;

public record RegisterResponseDto(string DeviceId, bool IsApproved, string Message);
