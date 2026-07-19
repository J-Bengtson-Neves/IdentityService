using IdentityService.Application.Common.Interfaces;
using IdentityService.Domain.Entities;
using System.Security.Cryptography;

namespace IdentityService.Infrastructure.Auth;

public class ApiKeyService : IApiKeyService
{
    public (ApiKey ApiKey, string PlainTextKey) GenerateApiKey(string userId, string name, List<string> scopes)
    {
        var buffer = new byte[32];
        RandomNumberGenerator.Fill(buffer);
        var randomString = Convert.ToBase64String(buffer)
            .Replace("+", "")
            .Replace("/", "")
            .Replace("=", "");
            
        var plainTextKey = $"identsvc_live_{randomString}";
        var prefix = plainTextKey.Substring(0, 15);
        var hash = HashApiKey(plainTextKey);

        var apiKey = new ApiKey
        {
            Name = name,
            KeyPrefix = prefix,
            KeyHash = hash,
            UserId = userId,
            Scopes = scopes
        };

        return (apiKey, plainTextKey);
    }

    public string HashApiKey(string plainTextKey)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(plainTextKey);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public bool ValidateApiKey(string plainTextKey, string hashedKey)
    {
        var computedHash = HashApiKey(plainTextKey);
        return CryptographicOperations.FixedTimeEquals(
            Convert.FromHexString(computedHash),
            Convert.FromHexString(hashedKey)
        );
    }
}
