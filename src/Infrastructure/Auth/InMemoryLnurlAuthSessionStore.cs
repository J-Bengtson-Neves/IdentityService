using System.Collections.Concurrent;
using IdentityService.Application.Auth;
using IdentityService.Application.Common.Interfaces;

namespace IdentityService.Infrastructure.Auth;

public class InMemoryLnurlAuthSessionStore : ILnurlAuthSessionStore
{
    private class SessionState
    {
        public bool IsCompleted { get; set; }
        public AuthResultDto? Result { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    private readonly ConcurrentDictionary<string, SessionState> _sessions = new();

    public void AddSession(string k1)
    {
        // Cleanup old sessions occasionally (simple approach for MVP)
        if (_sessions.Count > 1000)
        {
            var oldSessions = _sessions.Where(x => (DateTime.UtcNow - x.Value.CreatedAt).TotalMinutes > 15).Select(x => x.Key).ToList();
            foreach (var key in oldSessions)
            {
                _sessions.TryRemove(key, out _);
            }
        }

        _sessions.TryAdd(k1, new SessionState());
    }

    public void CompleteSession(string k1, AuthResultDto authResult)
    {
        if (_sessions.TryGetValue(k1, out var session))
        {
            session.Result = authResult;
            session.IsCompleted = true;
        }
    }

    public AuthResultDto? GetSessionResult(string k1)
    {
        if (_sessions.TryGetValue(k1, out var session) && session.IsCompleted)
        {
            var result = session.Result;
            // Optionally remove session after it is consumed
            _sessions.TryRemove(k1, out _);
            return result;
        }

        return null;
    }

    public bool IsSessionPending(string k1)
    {
        return _sessions.TryGetValue(k1, out var session) && !session.IsCompleted;
    }
}
