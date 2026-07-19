using IdentityService.Domain.Common;
using IdentityService.Domain.Entities;

namespace IdentityService.Domain.Events;

public class ApiKeyCreatedEvent : BaseEvent
{
    public ApiKey ApiKey { get; }

    public ApiKeyCreatedEvent(ApiKey apiKey)
    {
        ApiKey = apiKey;
    }
}
