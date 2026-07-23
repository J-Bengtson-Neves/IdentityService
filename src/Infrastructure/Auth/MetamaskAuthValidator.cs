using System.Text.RegularExpressions;

namespace IdentityService.Infrastructure.Auth;

public static partial class MetamaskAuthValidator
{
    private static readonly Regex AddressRegex = new("^0x[a-fA-F0-9]{40}$", RegexOptions.Compiled);
    private static readonly Regex SignatureRegex = new("^0x[a-fA-F0-9]{130}$", RegexOptions.Compiled);

    public static bool IsValidAddress(string? address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return false;
        }

        return AddressRegex.IsMatch(address.Trim());
    }

    public static bool IsValidSignature(string? signature)
    {
        if (string.IsNullOrWhiteSpace(signature))
        {
            return false;
        }

        return SignatureRegex.IsMatch(signature.Trim());
    }

    public static string NormalizeAddress(string? address)
    {
        return address?.Trim().ToLowerInvariant() ?? string.Empty;
    }
}
