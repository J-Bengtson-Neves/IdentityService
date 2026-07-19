using IdentityService.Application.Common.Interfaces;
using MediatR;

namespace IdentityService.Application.Auth.Commands.LnurlAuthCallback;

public record LnurlCallbackResponse(string Status);

public record LnurlAuthCallbackCommand(string Sig, string K1, string Key) : IRequest<LnurlCallbackResponse>;

public class LnurlAuthCallbackCommandHandler : IRequestHandler<LnurlAuthCallbackCommand, LnurlCallbackResponse>
{
    private readonly ILnurlAuthService _lnurlAuthService;
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;
    private readonly ILnurlAuthSessionStore _sessionStore;

    public LnurlAuthCallbackCommandHandler(ILnurlAuthService lnurlAuthService, IIdentityService identityService, ITokenService tokenService, ILnurlAuthSessionStore sessionStore)
    {
        _lnurlAuthService = lnurlAuthService;
        _identityService = identityService;
        _tokenService = tokenService;
        _sessionStore = sessionStore;
    }

    public async Task<LnurlCallbackResponse> Handle(LnurlAuthCallbackCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate signature using secp256k1
        var isValid = _lnurlAuthService.VerifySignature(request.Sig, request.K1, request.Key);
        
        if (!isValid)
        {
            return new LnurlCallbackResponse("ERROR");
        }

        // 2. Check if user exists by pubkey, or create a new one (as requested by user)
        var userId = await _identityService.GetUserIdByPubKeyAsync(request.Key); 
        
        if (string.IsNullOrEmpty(userId))
        {
            var (result, newUserId) = await _identityService.CreateUserWithoutEmailAsync(request.Key);
            if (!result.Succeeded) return new LnurlCallbackResponse("ERROR");
            userId = newUserId;
        }

        var accessToken = _tokenService.GenerateAccessToken(userId, "", new List<string>(), new List<System.Security.Claims.Claim>());
        var refreshToken = _tokenService.GenerateRefreshToken(userId, "0.0.0.0");

        var authResult = new AuthResultDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = refreshToken.ExpiresAt,
            UserId = userId,
            Email = null
        };

        _sessionStore.CompleteSession(request.K1, authResult);

        return new LnurlCallbackResponse("OK");
    }
}
