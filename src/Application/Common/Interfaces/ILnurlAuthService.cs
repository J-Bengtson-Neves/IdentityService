namespace IdentityService.Application.Common.Interfaces;

public interface ILnurlAuthService
{
    string GenerateChallenge();
    bool VerifySignature(string sig, string k1, string key);
    string GenerateLnurl(string callbackUrl, string k1);
}
