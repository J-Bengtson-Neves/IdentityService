using Microsoft.AspNetCore.Identity;

namespace IdentityService.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public string Plan { get; set; } = "Free";
    
    // Lightning Network
    public string? LightningPubKey { get; set; }
    public DateTime? LightningLinkedAt { get; set; }
    
    // Ethereum
    public string? EthereumAddress { get; set; }
    public DateTime? EthereumLinkedAt { get; set; }
    
    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public string? LastLoginProvider { get; set; }
}
