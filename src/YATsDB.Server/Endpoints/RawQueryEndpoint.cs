using YATsDb.Core.Services;
using YATsDB.Server.Endpoints.Common;

namespace YATsDB.Server.Endpoints;

public static class RawQueryEndpoint
{
    public static void AddRawQueryEndpoint(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("/query/raw/{bucketName}",
                (string bucketName, RawStringDto content, IDalServices dalServices)
                    =>
                {
                    var queryParameters = new QueryParameters();

                    var r = dalServices.Query(bucketName, content.Value.Trim(), queryParameters);
                    return Results.Ok(new QueryResult(r));
                })
            .ExcludeFromDescription();
    }
}