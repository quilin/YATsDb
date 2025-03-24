using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using YATsDb.Core.Services;
using YATsDB.Server.Infrastructure.Validation;

namespace YATsDB.Server.Endpoints;

public static class ManagementPostBucketsEndpoint
{
    public record CreateBucketDto(string BucketName, string? Description);

    public static void AddManagementPostBucketsEndpoint(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("/management/bucket",
                ([FromBody] CreateBucketDto dto, IManagementService managementService)
                    =>
                {
                    managementService.CreateBucket(dto.BucketName, dto.Description);
                    return Results.Ok();
                })
            .WithTags(TagNames.Management)
            .AddEndpointFilter<ValidationFilter<CreateBucketDto>>();
    }

    public class CreateBucketDtoValidator : AbstractValidator<CreateBucketDto>
    {
        public CreateBucketDtoValidator()
        {
            RuleFor(t => t.BucketName)
                .NotEmpty()
                .NotNull()
                .MaximumLength(150)
                .Matches("^[A-Za-z0-9_-]+$");

            RuleFor(t => t.Description)
                .MaximumLength(2048);
        }
    }
}