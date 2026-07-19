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
}
