using YATsDb.Core.Services;

namespace YATsDB.Server.Endpoints;

public static class ManagementGetMeasurementsEndpoint
{
    public static void AddManagementGetMeasurementsEndpoint(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("/management/bucket/{bucketName}/measurement",
            (string bucketName, IManagementService managementService)
                => Results.Ok((object?)managementService.ListMeasurements(bucketName))).WithTags(TagNames.Management);
    }
}