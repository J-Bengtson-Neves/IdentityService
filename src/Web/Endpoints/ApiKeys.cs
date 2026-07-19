using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Web.Endpoints;

public class ApiKeys : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(ListApiKeys, "keys").RequireAuthorization();
    }

    public static async Task<IResult> ListApiKeys(ISender sender)
    {
        // Placeholder for ListApiKeysQuery
        return TypedResults.Ok(new { message = "API Keys list" });
    }
}
