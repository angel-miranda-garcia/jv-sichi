namespace Domain.Entities;

public class AuditLog
{
    public int Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int EntityId { get; set; }
    public string Detail { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
