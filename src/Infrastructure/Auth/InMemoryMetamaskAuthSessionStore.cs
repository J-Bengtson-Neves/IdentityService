using System.Collections.Concurrent;

namespace IdentityService.Infrastructure.Auth;

public class InMemoryMetamaskAuthSessionStore : IMetamaskAuthSessionStore
{
    private readonly ConcurrentDictionary<string, (string Nonce, DateTime Expiry)> _store = new(StringComparer.OrdinalIgnoreCase);

    public void SaveNonce(string address, string nonce, TimeSpan expiry)
    {
        _store[address] = (nonce, DateTime.UtcNow.Add(expiry));
        Cleanup();
    }

    public string? GetNonce(string address)
    {
        if (_store.TryGetValue(address, out var session))
        {
            if (session.Expiry > DateTime.UtcNow)
            {
                return session.Nonce;
            }
            _store.TryRemove(address, out _); // Expired
        }
        return null;
    }

    public void ClearNonce(string address)
    {
        _store.TryRemove(address, out _);
    }

    private void Cleanup()
    {
        var now = DateTime.UtcNow;
        foreach (var kvp in _store)
        {
            if (kvp.Value.Expiry <= now)
            {
                _store.TryRemove(kvp.Key, out _);
            }
        }
    }
}
