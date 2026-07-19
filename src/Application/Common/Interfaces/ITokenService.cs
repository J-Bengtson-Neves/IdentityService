using IdentityService.Domain.Entities;

namespace IdentityService.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(string userId, string email, IList<string> roles, IList<System.Security.Claims.Claim> claims);
    RefreshToken GenerateRefreshToken(string userId, string ipAddress);
    System.Security.Claims.ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}
