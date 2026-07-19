using IdentityService.Domain.Common;

namespace IdentityService.Domain.Events;

public class LightningWalletLinkedEvent : BaseEvent
{
    public string UserId { get; }
    public string PubKey { get; }

    public LightningWalletLinkedEvent(string userId, string pubKey)
    {
        UserId = userId;
        PubKey = pubKey;
    }
}
