using IdentityService.Application.Common.Interfaces;
using MediatR;

namespace IdentityService.Application.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<AuthResultDto>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResultDto>
{
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(IIdentityService identityService, ITokenService tokenService)
    {
        _identityService = identityService;
        _tokenService = tokenService;
    }

    public async Task<AuthResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var isValid = await _identityService.CheckPasswordAsync(request.Email, request.Password);

        if (!isValid)
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        var userInfo = await _identityService.GetUserInfoAsync(request.Email);
        var userId = userInfo?.UserId ?? await _identityService.GetUserIdAsync(request.Email) ?? "";
        
        var customClaims = new List<System.Security.Claims.Claim>
        {
            new System.Security.Claims.Claim("plan", userInfo?.Plan ?? "Free")
        };
        
        if (!string.IsNullOrEmpty(userInfo?.DisplayName))
            customClaims.Add(new System.Security.Claims.Claim("name", userInfo.DisplayName));
            
        if (!string.IsNullOrEmpty(userInfo?.AvatarUrl))
            customClaims.Add(new System.Security.Claims.Claim("picture", userInfo.AvatarUrl));

        var roles = userInfo?.Roles ?? new List<string>();

        var accessToken = _tokenService.GenerateAccessToken(userId, request.Email, roles, customClaims);
        var refreshToken = _tokenService.GenerateRefreshToken(userId, "0.0.0.0"); // TODO: Get from accessor

        return new AuthResultDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = refreshToken.ExpiresAt,
            UserId = userId,
            Email = request.Email
        };
    }
}
