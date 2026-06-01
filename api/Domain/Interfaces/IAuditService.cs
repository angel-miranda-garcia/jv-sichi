namespace Domain.Interfaces;

public interface IAuditService
{
    Task LogAsync(string action, int userId, int entityId, string detail);
}
