using NCronJob;
using YATsDB.Server.Services.Contracts;

namespace YATsDB.Server.Services.Implementation;

public class CronStartupRegisterJob : IJob
{
    private readonly ILogger<CronStartupRegisterJob> logger;
    private readonly IServiceProvider serviceProvider;

    public CronStartupRegisterJob(ILogger<CronStartupRegisterJob> logger, IServiceProvider serviceProvider)
    {
        this.logger = logger;
        this.serviceProvider = serviceProvider;
    }

    public Task RunAsync(JobExecutionContext context, CancellationToken token)
    {
        logger.LogTrace("Entering to RunAsync");

        var cronManagement = serviceProvider.GetRequiredService<ICronManagement>();
        cronManagement.RegisterJobsOnStartup();

        return Task.CompletedTask;
    }
}