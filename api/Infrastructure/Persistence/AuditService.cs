using Domain.Entities;
using Domain.Interfaces;

namespace Infrastructure.Persistence;

public class AuditService : IAuditService
{
    private readonly SichiDbContext _db;

    public AuditService(SichiDbContext db)
    {
        _db = db;
    }

    public async Task LogAsync(string action, int userId, int entityId, string detail)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            Action = action,
            UserId = userId,
            EntityId = entityId,
            Detail = detail,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
    }
}
