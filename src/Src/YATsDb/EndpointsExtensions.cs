using FluentValidation;
using YATsDb.Endpoints;
using YATsDb.Endpoints.Discovery;

namespace YATsDb;

internal static class EndpointsExtensions
{
    public static void AddAppEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.AddBroadcastEndpoint();
        endpointRouteBuilder.AddNeighborsEndpoint();
        endpointRouteBuilder.AddRumorEndpoint();

        endpointRouteBuilder.AddRawWriteDataEndpoint();
        endpointRouteBuilder.AddRawQueryEndpoint();

        endpointRouteBuilder.AddCronGetEndpoint();
        endpointRouteBuilder.AddCronPostEndpoint();
        endpointRouteBuilder.AddCronDeleteEndpoint();

        endpointRouteBuilder.AddManagementGetBucketsEndpoint();
        endpointRouteBuilder.AddManagementGetMeasurementsEndpoint();
        endpointRouteBuilder.AddManagementPostBucketsEndpoint();
        endpointRouteBuilder.AddManagementDeleteBucketsEndpoint();

        endpointRouteBuilder.AddPostQueryEndpoint();
    }

    public static void AddValidators(this IServiceCollection services)
    {
        services.AddScoped<IValidator<CronPostEndpoint.CreateCronDto>, CronPostEndpoint.CreateCronDtoValidator>();
        services.AddScoped<IValidator<ManagementPostBucketsEndpoint.CreateBucketDto>, ManagementPostBucketsEndpoint.CreateBucketDtoValidator>();
        services.AddScoped<IValidator<PostQueryEndpoint.QueryDal>, PostQueryEndpoint.QueryDalValidator>();
    }
}
