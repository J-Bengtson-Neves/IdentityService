using IdentityService.Application.Common.Interfaces;
using MediatR;

namespace IdentityService.Application.Auth.Commands.GoogleLogin;

public record GoogleLoginCommand(string IdToken) : IRequest<AuthResultDto>;

public class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, AuthResultDto>
{
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;

    public GoogleLoginCommandHandler(IGoogleAuthService googleAuthService, IIdentityService identityService, ITokenService tokenService)
    {
        _googleAuthService = googleAuthService;
        _identityService = identityService;
        _tokenService = tokenService;
    }

    public async Task<AuthResultDto> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        var payload = await _googleAuthService.ValidateTokenAsync(request.IdToken);
        if (payload == null)
            throw new UnauthorizedAccessException("Invalid Google Token");

        var userInfo = await _identityService.GetUserInfoAsync(payload.Email);
        string userId;
        
        if (userInfo == null)
        {
            var randomPassword = Guid.NewGuid().ToString() + "aA1!";
            var (result, newUserId) = await _identityService.CreateUserAsync(payload.Email, randomPassword, payload.Name, payload.Picture);
            if (!result.Succeeded) 
            {
                var errors = string.Join(", ", result.Errors);
                throw new Exception($"Failed to create user: {errors}");
            }
            userId = newUserId;
            // Fetch newly created user info
            userInfo = await _identityService.GetUserInfoAsync(payload.Email);
        }
        else
        {
            userId = userInfo.UserId;
        }

        var customClaims = new List<System.Security.Claims.Claim>
        {
            new System.Security.Claims.Claim("plan", userInfo?.Plan ?? "Free")
        };
        
        if (!string.IsNullOrEmpty(userInfo?.DisplayName))
            customClaims.Add(new System.Security.Claims.Claim("name", userInfo.DisplayName));
            
        if (!string.IsNullOrEmpty(userInfo?.AvatarUrl))
            customClaims.Add(new System.Security.Claims.Claim("picture", userInfo.AvatarUrl));

        var roles = userInfo?.Roles ?? new List<string>();

        var accessToken = _tokenService.GenerateAccessToken(userId, payload.Email, roles, customClaims);
        var refreshToken = _tokenService.GenerateRefreshToken(userId, "0.0.0.0");

        return new AuthResultDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = refreshToken.ExpiresAt,
            UserId = userId,
            Email = payload.Email
        };
    }
}
