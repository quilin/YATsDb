namespace YATsDB.Server.Services.Contracts;

public record CronJobInfo(string Name, string CronExpression, bool Enabled);