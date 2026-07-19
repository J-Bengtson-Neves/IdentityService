using IdentityService.Domain.Entities;

namespace IdentityService.Application.Common.Interfaces;

public interface IApiKeyService
{
    (ApiKey ApiKey, string PlainTextKey) GenerateApiKey(string userId, string name, List<string> scopes);
    string HashApiKey(string plainTextKey);
    bool ValidateApiKey(string plainTextKey, string hashedKey);
}
