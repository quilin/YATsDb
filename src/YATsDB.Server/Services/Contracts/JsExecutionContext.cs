namespace YATsDB.Server.Services.Contracts;

public record JsExecutionContext(string BucketName, string Name, string Code, bool CheckOnly);