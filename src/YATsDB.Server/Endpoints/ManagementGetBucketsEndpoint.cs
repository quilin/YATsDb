using YATsDb.Core.Services;

namespace YATsDB.Server.Endpoints;

public static class ManagementGetBucketsEndpoint
{
    public static void AddManagementGetBucketsEndpoint(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("/management/bucket", (IManagementService managementService)
            =>
        {
            var buckets = managementService.ListBuckets();
            return Results.Ok(buckets);
        }).WithTags(TagNames.Management);
    }
}