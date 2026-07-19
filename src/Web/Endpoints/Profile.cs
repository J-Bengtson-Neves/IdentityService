using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Web.Endpoints;

public class Profile : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetProfile, "profile").RequireAuthorization();
    }

    public static async Task<IResult> GetProfile(ISender sender)
    {
        // Placeholder for GetProfileQuery
        return TypedResults.Ok(new { message = "Profile data" });
    }
}
