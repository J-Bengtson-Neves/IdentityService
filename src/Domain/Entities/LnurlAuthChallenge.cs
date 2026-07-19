using IdentityService.Domain.Common;

namespace IdentityService.Domain.Entities;

public class LnurlAuthChallenge : BaseEntity
{
    public string K1 { get; set; } = null!; // Challenge hex (32 bytes random)
    public string? LinkedPubKey { get; set; } // Preenchido após auth
    public string? UserId { get; set; } // Para vincular a user existente
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } // TTL curto (~5 min)
    public bool IsUsed { get; set; }
    public string Action { get; set; } = "login"; // "login", "link", "register"
}
