using IdentityService.Application.Auth;
using IdentityService.Application.Auth.Commands.GoogleLogin;
using IdentityService.Application.Auth.Commands.Login;
using IdentityService.Application.Auth.Commands.LnurlAuthCallback;
using IdentityService.Application.Auth.Queries.LnurlAuthChallenge;
using IdentityService.Application.Auth.Queries.LnurlAuthStatus;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Web.Endpoints;

public class Auth : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(Login, "login");
        groupBuilder.MapPost(Google, "google");
        groupBuilder.MapGet(LnurlChallenge, "lnurl-auth");
        groupBuilder.MapGet(LnurlCallback, "lnurl-callback");
        groupBuilder.MapGet(LnurlStatus, "lnurl-status");

        groupBuilder.MapPost(MetamaskNonce, "metamask-nonce");
        groupBuilder.MapPost(MetamaskLogin, "metamask-login");
    }

    public static async Task<AuthResultDto> Login(ISender sender, [FromBody] LoginCommand command)
    {
        return await sender.Send(command);
    }

    public static async Task<AuthResultDto> Google(ISender sender, [FromBody] GoogleLoginCommand command)
    {
        return await sender.Send(command);
    }

    public static async Task<LnurlAuthChallengeResponse> LnurlChallenge(ISender sender)
    {
        return await sender.Send(new LnurlAuthChallengeQuery());
    }

    public static async Task<LnurlCallbackResponse> LnurlCallback(ISender sender, [FromQuery] string k1, [FromQuery] string sig, [FromQuery] string key)
    {
        return await sender.Send(new LnurlAuthCallbackCommand(sig, k1, key));
    }

    public static async Task<LnurlAuthStatusResponse> LnurlStatus(ISender sender, [FromQuery] string k1)
    {
        return await sender.Send(new IdentityService.Application.Auth.Queries.LnurlAuthStatus.LnurlAuthStatusQuery(k1));
    }

    public static IResult MetamaskNonce([FromServices] IdentityService.Infrastructure.Auth.IMetamaskAuthService authService, [FromBody] MetamaskNonceRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Address)) return Results.BadRequest("Address is required");

        try
        {
            var nonce = authService.GenerateNonce(request.Address);
            return Results.Ok(new { Nonce = nonce });
        }
        catch (ArgumentException)
        {
            return Results.BadRequest("Invalid Ethereum address.");
        }
    }

    public static async Task<IResult> MetamaskLogin(
        [FromServices] IdentityService.Infrastructure.Auth.IMetamaskAuthService authService,
        [FromServices] IdentityService.Application.Common.Interfaces.IIdentityService identityService,
        [FromServices] IdentityService.Application.Common.Interfaces.ITokenService tokenService,
        [FromBody] MetamaskLoginRequest request)
    {
        var (isValid, userId) = await authService.VerifySignatureAndLoginAsync(request.Address, request.Signature);
        if (!isValid || string.IsNullOrEmpty(userId)) return Results.Unauthorized();

        // Need user info for claims
        var userName = await identityService.GetUserNameAsync(userId);
        var userInfo = await identityService.GetUserInfoAsync(userName ?? request.Address);

        var customClaims = new List<System.Security.Claims.Claim>
        {
            new System.Security.Claims.Claim("plan", userInfo?.Plan ?? "Free")
        };

        var accessToken = tokenService.GenerateAccessToken(userId, userName ?? request.Address, userInfo?.Roles ?? new List<string>(), customClaims);
        var refreshToken = tokenService.GenerateRefreshToken(userId, "0.0.0.0");

        return Results.Ok(new AuthResultDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = refreshToken.ExpiresAt,
            UserId = userId,
            Email = userName
        });
    }
}

public class MetamaskNonceRequest
{
    public string Address { get; set; } = null!;
}

public class MetamaskLoginRequest
{
    public string Address { get; set; } = null!;
    public string Signature { get; set; } = null!;
}
