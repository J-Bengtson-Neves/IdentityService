using IdentityService.Application.Common.Models;

namespace IdentityService.Application.Common.Interfaces;

public record UserInfoDto(string UserId, string Email, string? DisplayName, string? AvatarUrl, string Plan, IList<string> Roles);

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);
    Task<UserInfoDto?> GetUserInfoAsync(string email);
    Task<bool> IsInRoleAsync(string userId, string role);
    Task<bool> AuthorizeAsync(string userId, string policyName);
    Task<(Result Result, string UserId)> CreateUserAsync(string email, string password, string? displayName = null, string? avatarUrl = null);
    Task<Result> DeleteUserAsync(string userId);
    Task<string?> GetUserIdAsync(string userName);
    Task<string?> GetUserIdByPubKeyAsync(string pubKey);
    Task<bool> CheckPasswordAsync(string userName, string password);
    Task<(Result Result, string UserId)> CreateUserWithoutEmailAsync(string pubKey);
}

