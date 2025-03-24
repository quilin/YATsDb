using YATsDb.Core.Services;
using YATsDB.Server.Endpoints.Common;

namespace YATsDB.Server.Endpoints;

public static class RawWriteDataEndpoint
{
    public static void AddRawWriteDataEndpoint(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("/write/{bucketName}",
                (string bucketName, RawStringDto content, IDalServices dalServices)
                    =>
                {
                    dalServices.InsertLines(bucketName, content.Value);
                    return Results.Created();
                })
            .ExcludeFromDescription();
    }
}