using IdentityService.Application.Common.Models;

namespace IdentityService.Infrastructure.Auth;

public interface IMetamaskAuthService
{
    string GenerateNonce(string address);
    Task<(bool IsValid, string? UserId)> VerifySignatureAndLoginAsync(string address, string signature);
}
