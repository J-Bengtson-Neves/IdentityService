using IdentityService.Application.Common.Interfaces;
using Nethereum.Signer;

namespace IdentityService.Infrastructure.Auth;

public class MetamaskAuthService : IMetamaskAuthService
{
    private readonly IMetamaskAuthSessionStore _sessionStore;
    private readonly IIdentityService _identityService;

    public MetamaskAuthService(IMetamaskAuthSessionStore sessionStore, IIdentityService identityService)
    {
        _sessionStore = sessionStore;
        _identityService = identityService;
    }

    public string GenerateNonce(string address)
    {
        if (!MetamaskAuthValidator.IsValidAddress(address))
        {
            throw new ArgumentException("Invalid Ethereum address.", nameof(address));
        }

        var normalizedAddress = MetamaskAuthValidator.NormalizeAddress(address);
        var nonce = Guid.NewGuid().ToString("N");
        _sessionStore.SaveNonce(normalizedAddress, nonce, TimeSpan.FromMinutes(5));
        return nonce;
    }

    public async Task<(bool IsValid, string? UserId)> VerifySignatureAndLoginAsync(string address, string signature)
    {
        if (!MetamaskAuthValidator.IsValidAddress(address) || !MetamaskAuthValidator.IsValidSignature(signature))
        {
            return (false, null);
        }

        var normalizedAddress = MetamaskAuthValidator.NormalizeAddress(address);
        var nonce = _sessionStore.GetNonce(normalizedAddress);
        if (string.IsNullOrEmpty(nonce))
        {
            return (false, null);
        }

        try
        {
            var signer = new EthereumMessageSigner();
            var recoveredAddress = signer.EncodeUTF8AndEcRecover(nonce, signature);

            if (string.Equals(recoveredAddress, normalizedAddress, StringComparison.OrdinalIgnoreCase))
            {
                _sessionStore.ClearNonce(normalizedAddress);

                var userId = await _identityService.GetUserIdByEthereumAddressAsync(normalizedAddress);
                if (string.IsNullOrEmpty(userId))
                {
                    var result = await _identityService.CreateUserWithEthereumAddressAsync(normalizedAddress);
                    if (result.Result.Succeeded)
                    {
                        userId = result.UserId;
                    }
                    else
                    {
                        return (false, null);
                    }
                }

                return (true, userId);
            }
        }
        catch (Exception)
        {
            // Invalid signature format
        }

        return (false, null);
    }
}
