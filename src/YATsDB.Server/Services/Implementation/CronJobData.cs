namespace YATsDB.Server.Services.Implementation;

internal class CronJobData
{
    public string BucketName { get; set; }

    public string Name { get; set; }

    public string CronExpression { get; set; }

    public string Code { get; set; }

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset? Updated { get; set; }

    public CronJobData()
    {
        BucketName = string.Empty;
        Name = string.Empty;
        CronExpression = string.Empty;
        Code = string.Empty;
    }
}