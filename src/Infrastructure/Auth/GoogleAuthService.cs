using IdentityService.Application.Common.Interfaces;

namespace IdentityService.Infrastructure.Auth;

public class GoogleAuthService : IGoogleAuthService
{
    public async Task<GoogleUserPayload?> ValidateTokenAsync(string idToken)
    {
        try
        {
            var payload = await Google.Apis.Auth.GoogleJsonWebSignature.ValidateAsync(idToken);
            return new GoogleUserPayload
            {
                Email = payload.Email,
                Name = payload.Name,
                Picture = payload.Picture,
                Subject = payload.Subject
            };
        }
        catch
        {
            return null;
        }
    }
}
