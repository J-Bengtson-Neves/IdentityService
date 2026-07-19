using IdentityService.Domain.Entities;

namespace IdentityService.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<ApiKey> ApiKeys { get; }
    DbSet<LnurlAuthChallenge> LnurlAuthChallenges { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
