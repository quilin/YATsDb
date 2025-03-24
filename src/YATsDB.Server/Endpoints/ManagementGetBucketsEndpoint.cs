using YATsDb.Core.Services;

namespace YATsDB.Server.Endpoints;

public static class ManagementGetBucketsEndpoint
{
    public record BucketInfoDto(string Name, string? Description, DateTimeOffset Created);

    public static void AddManagementGetBucketsEndpoint(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("/management/bucket", (IManagementService managementService)
            =>
        {
            var buckets = managementService.ListBuckets();
            var model = buckets.Select(t => new BucketInfoDto(t.Name, t.Description, t.Created))
                .ToList();
            return Results.Ok(model);
        }).WithTags(TagNames.Management);
    }
}