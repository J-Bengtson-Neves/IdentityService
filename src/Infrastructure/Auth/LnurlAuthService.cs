using IdentityService.Application.Common.Interfaces;
using IdentityService.Domain.Entities;

namespace IdentityService.Infrastructure.Auth;

public class LnurlAuthService : ILnurlAuthService
{
    public string GenerateChallenge()
    {
        var buffer = new byte[32];
        System.Security.Cryptography.RandomNumberGenerator.Fill(buffer);
        return Convert.ToHexString(buffer).ToLowerInvariant();
    }

    public bool VerifySignature(string sig, string k1, string key)
    {
        try
        {
            var pubKey = new NBitcoin.PubKey(Convert.FromHexString(key));
            var messageHash = System.Security.Cryptography.SHA256.HashData(Convert.FromHexString(k1));
            
            return pubKey.Verify(new NBitcoin.uint256(messageHash), Convert.FromHexString(sig));
        }
        catch
        {
            return false;
        }
    }

    public string GenerateLnurl(string callbackUrl, string k1)
    {
        var url = $"{callbackUrl}?tag=login&k1={k1}";
        var bytes = System.Text.Encoding.UTF8.GetBytes(url);
        
        // Convert 8-bit bytes to 5-bit words for Bech32
        var words = ConvertBits(bytes, 8, 5, true);
        
        // LNURL Bech32 encode
        var encoder = NBitcoin.DataEncoders.Encoders.Bech32("lnurl");
        return encoder.EncodeData(words, NBitcoin.DataEncoders.Bech32EncodingType.BECH32);
    }

    private static byte[] ConvertBits(IEnumerable<byte> data, int fromBits, int toBits, bool pad)
    {
        int acc = 0;
        int bits = 0;
        int maxv = (1 << toBits) - 1;
        var ret = new System.Collections.Generic.List<byte>();

        foreach (var b in data)
        {
            acc = (acc << fromBits) | b;
            bits += fromBits;
            while (bits >= toBits)
            {
                bits -= toBits;
                ret.Add((byte)((acc >> bits) & maxv));
            }
        }

        if (pad && bits > 0)
        {
            ret.Add((byte)((acc << (toBits - bits)) & maxv));
        }

        return ret.ToArray();
    }
}
