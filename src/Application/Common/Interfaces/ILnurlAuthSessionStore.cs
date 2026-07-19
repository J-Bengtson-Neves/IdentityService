using IdentityService.Application.Auth;

namespace IdentityService.Application.Common.Interfaces;

public interface ILnurlAuthSessionStore
{
    void AddSession(string k1);
    void CompleteSession(string k1, AuthResultDto authResult);
    AuthResultDto? GetSessionResult(string k1);
    bool IsSessionPending(string k1);
}
