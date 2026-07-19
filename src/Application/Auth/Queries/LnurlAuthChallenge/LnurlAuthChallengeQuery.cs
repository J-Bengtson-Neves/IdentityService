using IdentityService.Application.Common.Interfaces;
using MediatR;

namespace IdentityService.Application.Auth.Queries.LnurlAuthChallenge;

public record LnurlAuthChallengeResponse(string LnurlEncoded, string K1, string CallbackUrl);

public record LnurlAuthChallengeQuery : IRequest<LnurlAuthChallengeResponse>;

public class LnurlAuthChallengeQueryHandler : IRequestHandler<LnurlAuthChallengeQuery, LnurlAuthChallengeResponse>
{
    private readonly ILnurlAuthService _lnurlAuthService;
    private readonly ILnurlAuthSessionStore _sessionStore;

    public LnurlAuthChallengeQueryHandler(ILnurlAuthService lnurlAuthService, ILnurlAuthSessionStore sessionStore)
    {
        _lnurlAuthService = lnurlAuthService;
        _sessionStore = sessionStore;
    }

    public Task<LnurlAuthChallengeResponse> Handle(LnurlAuthChallengeQuery request, CancellationToken cancellationToken)
    {
        var k1 = _lnurlAuthService.GenerateChallenge();
        
        // Save session in memory
        _sessionStore.AddSession(k1);
        
        // Generate LNURL
        var callbackUrl = $"https://identity.local/api/auth/lnurl-callback?k1={k1}"; // Usually from IConfiguration
        var lnurl = _lnurlAuthService.GenerateLnurl(callbackUrl, k1);

        return Task.FromResult(new LnurlAuthChallengeResponse(lnurl, k1, callbackUrl));
    }
}
