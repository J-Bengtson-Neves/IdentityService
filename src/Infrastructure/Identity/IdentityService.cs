using IdentityService.Application.Common.Interfaces;
using IdentityService.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory;
    private readonly IAuthorizationService _authorizationService;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
        IAuthorizationService authorizationService)
    {
        _userManager = userManager;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _authorizationService = authorizationService;
    }

    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user?.UserName;
    }

    public async Task<UserInfoDto?> GetUserInfoAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email) ?? await _userManager.FindByNameAsync(email);
        if (user == null) return null;

        var roles = await _userManager.GetRolesAsync(user);
        return new UserInfoDto(user.Id, user.Email!, user.DisplayName, user.AvatarUrl, user.Plan, roles);
    }

    public async Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password, string? displayName = null, string? avatarUrl = null)
    {
        var user = new ApplicationUser
        {
            UserName = userName,
            Email = userName,
            DisplayName = displayName,
            AvatarUrl = avatarUrl,
            Plan = "Free"
        };

        var result = await _userManager.CreateAsync(user, password);

        return (result.ToApplicationResult(), user.Id);
    }

    public async Task<(Result Result, string UserId)> CreateUserWithoutEmailAsync(string pubKey)
    {
        var user = new ApplicationUser
        {
            UserName = Guid.NewGuid().ToString("N"), // No email, random username
            LightningPubKey = pubKey,
            LightningLinkedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user);

        return (result.ToApplicationResult(), user.Id);
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null && await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return false;
        }

        var principal = await _userClaimsPrincipalFactory.CreateAsync(user);

        var result = await _authorizationService.AuthorizeAsync(principal, policyName);

        return result.Succeeded;
    }

    public async Task<Result> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null ? await DeleteUserAsync(user) : Result.Success();
    }

    public async Task<Result> DeleteUserAsync(ApplicationUser user)
    {
        var result = await _userManager.DeleteAsync(user);

        return result.ToApplicationResult();
    }

    public async Task<string?> GetUserIdAsync(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);
        return user?.Id;
    }

    public async Task<string?> GetUserIdByPubKeyAsync(string pubKey)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.LightningPubKey == pubKey);
        return user?.Id;
    }

    public async Task<bool> CheckPasswordAsync(string userName, string password)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user == null) return false;
        
        return await _userManager.CheckPasswordAsync(user, password);
    }
}
