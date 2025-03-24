using NCronJob;
using YATsDB.Server.Services.Contracts;

namespace YATsDB.Server.Services.Implementation;

public class CronTriggerJob : IJob
{
    private readonly ICronManagement cronManagement;
    private readonly ILogger<CronTriggerJob> logger;

    public CronTriggerJob(ICronManagement cronManagement, ILogger<CronTriggerJob> logger)
    {
        this.cronManagement = cronManagement;
        this.logger = logger;
    }

    public Task RunAsync(JobExecutionContext context, CancellationToken token)
    {
        logger.LogInformation("Run JOB {name}", context.Parameter);
        System.Diagnostics.Debug.Assert(context.Parameter is string);

        var jobName = context.Parameter.ToString()!;
        var parts = jobName.Split(':', 2);

        try
        {
            cronManagement.ExecuteJob(parts[0], parts[1]);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during processing job {name}.", jobName);
        }

        return Task.CompletedTask;
    }
}