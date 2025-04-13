using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using YATsDb.Core;
using YATsDb.Core.HighLevel;
using YATsDb.Core.Services;

namespace YATsDb.Client.Http;

internal sealed class ManagementService : IManagementService
{
    private const string BucketsPath = "/management/bucket";
    private const string DeleteBucketPath = "/management/bucket/{0}";
    private const string MeasurementsPath = "/management/bucket/{0}/measurement";

    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogger<ManagementService> logger;


    public ManagementService(IHttpClientFactory httpClientFactory, ILogger<ManagementService> logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;
    }

    public void CreateBucket(string name, string? description)
    {
        var client = httpClientFactory.CreateClient(Settings.ClientName);

        using var content = JsonContent.Create(new CreateBucketDto(name, description));
        var response = client.PostAsync(BucketsPath, content).ConfigureAwait(false).GetAwaiter().GetResult();

        if (!response.IsSuccessStatusCode)
        {
            var responseMessage = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            logger.LogError("Failed to create bucket {BucketName}, message {Message}", name,
                responseMessage);

            throw new YatsdbException("Failed to create bucket");
        }

        logger.LogDebug("Created bucket {BucketName}", name);
    }

    public void DeleteBucket(string name)
    {
        var client = httpClientFactory.CreateClient(Settings.ClientName);
        var path = string.Format(DeleteBucketPath, name);

        var response = client.DeleteAsync(path).ConfigureAwait(false).GetAwaiter().GetResult();

        if (!response.IsSuccessStatusCode)
        {
            var responseMessage = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            logger.LogError("Failed to delete bucket {BucketName}, message {Message}", name,
                responseMessage);

            throw new YatsdbException("Failed to delete bucket");
        }

        logger.LogDebug("Deleted bucket {BucketName}", name);
    }

    public List<HighLevelBucketInfo> ListBuckets()
    {
        var client = httpClientFactory.CreateClient(Settings.ClientName);

        var response = client.GetAsync(BucketsPath).ConfigureAwait(false).GetAwaiter().GetResult();

        if (!response.IsSuccessStatusCode)
        {
            var responseMessage = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            logger.LogError("Failed to list buckets, message {Message}", responseMessage);

            throw new YatsdbException("Failed to list buckets");
        }

        var result = response.Content.ReadFromJsonAsync<List<HighLevelBucketInfo>>().ConfigureAwait(false)
            .GetAwaiter().GetResult();

        logger.LogDebug("Retrieved bucket list");

        return result ?? [];
    }

    public List<string> ListMeasurements(string bucketName)
    {
        var client = httpClientFactory.CreateClient(Settings.ClientName);
        var path = string.Format(MeasurementsPath, bucketName);

        var response = client.GetAsync(path).ConfigureAwait(false).GetAwaiter().GetResult();

        if (!response.IsSuccessStatusCode)
        {
            var responseMessage = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            logger.LogError("Failed to list measurements, message {Message}", responseMessage);

            throw new YatsdbException("Failed to list measurements");
        }

        var result = response.Content.ReadFromJsonAsync<List<string>>().ConfigureAwait(false)
            .GetAwaiter().GetResult();

        logger.LogDebug("Retrieved measurements list for bucket {BucketName}", bucketName);

        return result ?? [];
    }
}
