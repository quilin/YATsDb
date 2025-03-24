using System.Text.Json;
using NCronJob;
using YATsDb.Core;
using YATsDb.Core.LowLevel;
using YATsDB.Server.Services.Contracts;

namespace YATsDB.Server.Services.Implementation;

public class CronManagement : ICronManagement
{
    private readonly IKvStorage kvStorage;
    private readonly IRuntimeJobRegistry runtimeJobRegistry;
    private readonly IJsInternalEngine jsInternalEngine;
    private readonly TimeProvider timeProvider;
    private readonly ILogger<CronManagement> logger;

    #region Keys

    private const string MainKey = "CronJob:enabled";
    private const string DataKey = "CronJob:data";

    #endregion

    public CronManagement(IKvStorage kvStorage,
        IRuntimeJobRegistry runtimeJobRegistry,
        IJsInternalEngine jsInternalEngine,
        TimeProvider timeProvider,
        ILogger<CronManagement> logger)
    {
        this.kvStorage = kvStorage;
        this.runtimeJobRegistry = runtimeJobRegistry;
        this.jsInternalEngine = jsInternalEngine;
        this.timeProvider = timeProvider;
        this.logger = logger;
    }

    public void CreateCronJob(string bucketName, CreateCronJobRequest request)
    {
        logger.LogTrace("Entering to CreateCronJob with name {bucketName}/{name}.", bucketName, request.Name);

        var fullName = BuildFullJobName(bucketName, request.Name);
        var cronJobData = new CronJobData()
        {
            Code = request.Code,
            BucketName = bucketName,
            Created = timeProvider.GetUtcNow(),
            Name = request.Name.Trim(),
            CronExpression = request.CronExpression.Trim(),
            Updated = null
        };

        if (kvStorage.TryGet(MainKey, fullName, out _))
        {
            throw new YatsdbDataException($"Job {bucketName}/{request.Name} already exists.");
        }

        var jsonData = JsonSerializer.Serialize(cronJobData);

        kvStorage.Upsert(MainKey, fullName, request.Enabled.ToString());
        kvStorage.Upsert(DataKey, fullName, jsonData);

        logger.LogInformation("Created a new CronJob {bucketName}/{name}.", bucketName, cronJobData.Name);

        if (request.Enabled)
        {
            EnableCronJob(bucketName, cronJobData.Name, cronJobData.CronExpression);
        }
    }

    public void DeleteCronJob(string bucketName, string jobName)
    {
        logger.LogTrace("Entering to DeleteCronJob with name {bucketName}/{name}.", bucketName, jobName);

        jobName = jobName.Trim();
        var jobFullName = BuildFullJobName(bucketName, jobName);
        if (!kvStorage.TryGet(MainKey, jobFullName, out var isEnabledStr))
        {
            throw new YatsdbDataException($"Job {bucketName}/{jobName} does not exists");
        }

        if (Convert.ToBoolean(isEnabledStr))
        {
            DisableCronJob(bucketName, jobName);
        }

        kvStorage.Remove(MainKey, jobFullName);
        kvStorage.Remove(DataKey, jobFullName);

        logger.LogInformation("Remove Cron json {bucketName}/{name}.", bucketName, jobName);
        //TODO: check removing
    }

    public void DeleteCronJobs(string bucketName)
    {
        logger.LogTrace("Entering to DeleteCronJobs with name {bucketName}.", bucketName);

        foreach ((var fullName, var enableStr) in kvStorage.EnumerateKeyValues(MainKey))
        {
            if (Convert.ToBoolean(enableStr))
            {
                if (!kvStorage.TryGet(DataKey, fullName, out var dataString))
                {
                    logger.LogWarning("Incompatible data for cron job {name}.", fullName);
                    continue;
                }

                var cronJobData = JsonSerializer.Deserialize<CronJobData>(dataString);
                System.Diagnostics.Debug.Assert(cronJobData != null);

                DeleteCronJob(cronJobData.BucketName, cronJobData.Name);
            }
        }
    }

    public void UpdateCronJob(string bucketName, CreateCronJobRequest request)
    {
        logger.LogTrace("Entering to UpdateCronJob with name {bucketName}/{name}.", bucketName, request.Name);

        var jobName = request.Name.Trim();
        var jobFullName = BuildFullJobName(bucketName, jobName);
        if (!kvStorage.TryGet(MainKey, jobFullName, out var isEnabledStr))
        {
            throw new YatsdbDataException("Job does not exists"); //TODO
        }

        if (!kvStorage.TryGet(DataKey, jobFullName, out var dataString))
        {
            logger.LogError("Incompatible data for cron job {bucketName}/{name}.", bucketName, jobName);
            throw new YatsdbDataException("Job does not exists"); //TODO
        }

        if (Convert.ToBoolean(isEnabledStr))
        {
            DisableCronJob(bucketName, jobName);
        }

        var cronJobData = JsonSerializer.Deserialize<CronJobData>(dataString);
        System.Diagnostics.Debug.Assert(cronJobData != null);

        cronJobData.CronExpression = request.CronExpression.Trim();
        cronJobData.Code = request.Code.Trim();
        cronJobData.Updated = timeProvider.GetUtcNow();

        var jsonData = JsonSerializer.Serialize(cronJobData);

        kvStorage.Upsert(MainKey, jobName, request.Enabled.ToString());
        kvStorage.Upsert(DataKey, jobName, jsonData);

        if (request.Enabled)
        {
            EnableCronJob(bucketName, jobName, cronJobData.CronExpression);
        }
    }

    public List<CronJobInfo> ListJobs(string bucketName)
    {
        logger.LogTrace("Entering to ListJobs by {bucketName}.", bucketName);

        var prefix = string.Concat(bucketName, ":");
        var result = new List<CronJobInfo>();
        foreach ((var fullName, var enableStr) in kvStorage.EnumerateKeyValues(MainKey))
        {
            if (fullName.StartsWith(prefix))
            {
                if (!kvStorage.TryGet(DataKey, fullName, out var dataString))
                {
                    logger.LogError("Incompatible data for cron job {name}.", fullName);
                    throw new YatsdbDataException("invalid data");
                }

                var cronJobData = JsonSerializer.Deserialize<CronJobData>(dataString);
                System.Diagnostics.Debug.Assert(cronJobData != null);

                result.Add(new CronJobInfo(cronJobData.Name,
                    cronJobData.CronExpression,
                    Convert.ToBoolean(enableStr)));
            }
        }

        return result;
    }

    public CronJob? TryGetCronJob(string bucketName, string jobName)
    {
        logger.LogTrace("Entering to TryGetCronJob with name {bucketName}/{name}.", bucketName, jobName);

        jobName = jobName.Trim();
        var jobFullName = BuildFullJobName(bucketName, jobName);
        if (!kvStorage.TryGet(MainKey, jobFullName, out var isEnabledStr))
        {
            logger.LogDebug("CronJob {bucketName}/{name} not found.", bucketName, jobName);
            return null;
        }

        if (!kvStorage.TryGet(DataKey, jobFullName, out var dataString))
        {
            logger.LogError("Incompatible data for cron job {bucketName}/{name}.", bucketName, jobName);
            throw new YatsdbDataException("Incompatible data");
        }

        var cronJobData = JsonSerializer.Deserialize<CronJobData>(dataString);
        System.Diagnostics.Debug.Assert(cronJobData != null);

        return new CronJob(cronJobData.BucketName,
            cronJobData.Name,
            cronJobData.CronExpression,
            cronJobData.Code,
            Convert.ToBoolean(isEnabledStr),
            cronJobData.Created,
            cronJobData.Updated);
    }

    public void RegisterJobsOnStartup()
    {
        logger.LogTrace("Entering to RegisterJobsOnStartup.");

        foreach ((var fullName, var enableStr) in kvStorage.EnumerateKeyValues(MainKey))
        {
            if (Convert.ToBoolean(enableStr))
            {
                if (!kvStorage.TryGet(DataKey, fullName, out var dataString))
                {
                    logger.LogWarning("Incompatible data for cron job {name}.", fullName);
                    continue;
                }

                var cronJobData = JsonSerializer.Deserialize<CronJobData>(dataString);
                System.Diagnostics.Debug.Assert(cronJobData != null);

                EnableCronJob(cronJobData.BucketName, cronJobData.Name, cronJobData.CronExpression);
            }
        }
    }

    public void ExecuteJob(string bucketName, string jobName)
    {
        var jobDefinition = TryGetCronJob(bucketName, jobName);
        if (jobDefinition == null)
        {
            logger.LogDebug("CronJob {bucketName}/{name} not found.", bucketName, jobName);
            throw new YatsdbDataException($"CronJob {bucketName}/{jobName} not found."); //TODO
        }

        var ctx = new JsExecutionContext(jobDefinition.BucketName,
            jobDefinition.Name,
            jobDefinition.Code,
            false);

        jsInternalEngine.ExecuteModule(ctx);
    }

    private void EnableCronJob(string bucketName, string name, string cronExpression)
    {
        var jobFullName = BuildFullJobName(bucketName, name);

        var ttt = runtimeJobRegistry.TryGetSchedule(jobFullName, out var aaa, out _);
        if (ttt)
        {
            runtimeJobRegistry.EnableJob(jobFullName);
        }
        else
        {
            runtimeJobRegistry.AddJob(builder =>
            {
                builder.AddJob<CronTriggerJob>(opt => opt.WithCronExpression(cronExpression)
                    .WithName(jobFullName)
                    .WithParameter(jobFullName));
            });
        }


        logger.LogInformation("Enabled CronJob {bucketName}/{name}.", bucketName, name);
    }

    private void DisableCronJob(string bucketName, string name)
    {
        runtimeJobRegistry.RemoveJob(BuildFullJobName(bucketName, name));
        logger.LogInformation("Remove Cron json scheduling {bucketName}/{name}.", bucketName, name);
    }

    private string BuildFullJobName(string bucketName, string name)
    {
        return string.Concat(bucketName, ":", name);
    }
}