namespace Application.Audit;

public record AuditLogDto(
    int Id,
    string Action,
    string UserEmail,
    int EntityId,
    string Detail,
    DateTime CreatedAt);
