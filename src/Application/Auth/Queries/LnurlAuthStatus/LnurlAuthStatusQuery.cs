using IdentityService.Application.Common.Interfaces;
using MediatR;

namespace IdentityService.Application.Auth.Queries.LnurlAuthStatus;

public record LnurlAuthStatusQuery(string K1) : IRequest<LnurlAuthStatusResponse>;

public record LnurlAuthStatusResponse(bool IsCompleted, AuthResultDto? Result);

public class LnurlAuthStatusQueryHandler : IRequestHandler<LnurlAuthStatusQuery, LnurlAuthStatusResponse>
{
    private readonly ILnurlAuthSessionStore _sessionStore;

    public LnurlAuthStatusQueryHandler(ILnurlAuthSessionStore sessionStore)
    {
        _sessionStore = sessionStore;
    }

    public Task<LnurlAuthStatusResponse> Handle(LnurlAuthStatusQuery request, CancellationToken cancellationToken)
    {
        var result = _sessionStore.GetSessionResult(request.K1);
        
        if (result != null)
        {
            return Task.FromResult(new LnurlAuthStatusResponse(true, result));
        }

        var isPending = _sessionStore.IsSessionPending(request.K1);
        
        if (!isPending)
        {
            // Session not found or expired
            throw new UnauthorizedAccessException("Session expired or invalid");
        }

        return Task.FromResult(new LnurlAuthStatusResponse(false, null));
    }
}
