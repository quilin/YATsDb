using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using YATsDb.Core;
using YATsDb.Core.Services;

namespace YATsDb.Client.Http;

public sealed class DalService : IDalService
{
    private const string InsertLinesPath = "/write/{0}";
    private const string QueryPath = "/query/raw/{0}";

    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogger<DalService> logger;

    public DalService(IHttpClientFactory httpClientFactory, ILogger<DalService> logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;
    }

    public void InsertLines(string bucketName, string lines)
    {
        var client = httpClientFactory.CreateClient(Settings.ClientName);
        var path = string.Format(InsertLinesPath, bucketName);

        using var content = new StringContent(lines);
        var response = client.PostAsync(path, content).ConfigureAwait(false).GetAwaiter().GetResult();

        if (!response.IsSuccessStatusCode)
        {
            var responseMessage = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            logger.LogError("Failed to insert lines into bucket {BucketName}, message {Message}", bucketName,
                responseMessage);
        }

        logger.LogDebug("Inserted lines into bucket {BucketName}", bucketName);
    }

    public List<object?[]> Query(string bucketName, string query, QueryParameters parameters)
    {
        var client = httpClientFactory.CreateClient(Settings.ClientName);
        var path = string.Format(QueryPath, bucketName);

        using var content = new StringContent(query);
        var response = client.PostAsync(path, content).ConfigureAwait(false).GetAwaiter().GetResult();

        if (!response.IsSuccessStatusCode)
        {
            var responseMessage = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            logger.LogError("Failed to query bucket {BucketName}, message {Message}", bucketName,
                responseMessage);
        }

        logger.LogDebug("Queried bucket {BucketName}", bucketName);

        try
        {
            var result = response.Content.ReadFromJsonAsync<QueryResult>().ConfigureAwait(false).GetAwaiter()
                .GetResult();

            return result?.Result ?? [];
        }
        catch (Exception e)
        {
            throw new YatsdbException("Failed to deserialize query result", e);
        }
    }
}
