namespace YATsDB.Server.Services.Contracts;

public record CreateCronJobRequest(
    string Name,
    string CronExpression,
    string Code,
    bool Enabled);