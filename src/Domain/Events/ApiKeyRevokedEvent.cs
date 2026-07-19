using IdentityService.Domain.Common;
using IdentityService.Domain.Entities;

namespace IdentityService.Domain.Events;

public class ApiKeyRevokedEvent : BaseEvent
{
    public ApiKey ApiKey { get; }

    public ApiKeyRevokedEvent(ApiKey apiKey)
    {
        ApiKey = apiKey;
    }
}
