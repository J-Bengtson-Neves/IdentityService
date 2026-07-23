namespace IdentityService.Infrastructure.Auth;

public interface IMetamaskAuthSessionStore
{
    void SaveNonce(string address, string nonce, TimeSpan expiry);
    string? GetNonce(string address);
    void ClearNonce(string address);
}
