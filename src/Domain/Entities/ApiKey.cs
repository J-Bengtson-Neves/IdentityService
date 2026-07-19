using IdentityService.Domain.Common;

namespace IdentityService.Domain.Entities;

public class ApiKey : BaseAuditableEntity
{
    public string Name { get; set; } = null!;
    public string KeyHash { get; set; } = null!;
    public string KeyPrefix { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public List<string> Scopes { get; set; } = new();
}
