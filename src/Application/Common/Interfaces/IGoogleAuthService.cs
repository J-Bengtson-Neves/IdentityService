namespace IdentityService.Application.Common.Interfaces;

public class GoogleUserPayload
{
    public string Email { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Picture { get; set; } = null!;
    public string Subject { get; set; } = null!; // Google User ID
}

public interface IGoogleAuthService
{
    Task<GoogleUserPayload?> ValidateTokenAsync(string idToken);
}
