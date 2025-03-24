namespace YATsDB.Server.Services.Contracts;

public interface IJsInternalEngine
{
    void ExecuteModule(JsExecutionContext context);
}