using System.Diagnostics.CodeAnalysis;
using Jint;
using Microsoft.Extensions.Options;
using YATsDb.Core.Services;
using YATsDB.Server.Services.Configuration;
using YATsDB.Server.Services.Contracts;

namespace YATsDB.Server.Services.Implementation.JsEngine;

public class JsInternalEngine : IJsInternalEngine
{
    private readonly ILoggerFactory loggerFactory;
    private readonly IDalServices dalServices;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly IOptions<JsEngineSetup> jsEngineSetup;
    private readonly ILogger<JsInternalEngine> logger;

    public JsInternalEngine(ILoggerFactory loggerFactory,
        IDalServices dalServices,
        IHttpClientFactory httpClientFactory,
        IOptions<JsEngineSetup> jsEngineSetup)
    {
        this.loggerFactory = loggerFactory;
        this.dalServices = dalServices;
        this.httpClientFactory = httpClientFactory;
        this.jsEngineSetup = jsEngineSetup;
        logger = loggerFactory.CreateLogger<JsInternalEngine>();
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, typeof(Engine))]
    public void ExecuteModule(JsExecutionContext context)
    {
        logger.LogTrace("Entering to ExecuteModule for name {name}.", context.Name);
        using var scope = logger.BeginScope("CronJobName: {cronJobName}", context.Name);


        using var engine = new Engine(options =>
        {
            options.LimitMemory(jsEngineSetup.Value.MemoryLimit);
            options.TimeoutInterval(jsEngineSetup.Value.Timeout);
            options.MaxStatements(jsEngineSetup.Value.MaxStatements);
            options.LimitRecursion(jsEngineSetup.Value.LimitRecursion);

            if (!string.IsNullOrEmpty(jsEngineSetup.Value.ModuleBasePath))
            {
                options.EnableModules(jsEngineSetup.Value.ModuleBasePath, true);
            }

            // Async calls https://github.com/sebastienros/jint/issues/1883#event-13140700291
            options.ExperimentalFeatures = ExperimentalFeature.TaskInterop;
        });

        var httpFunctions =
            new HttpFunctions(engine, httpClientFactory, jsEngineSetup.Value.Api.EnableHttpApi);
        var jsLog = new JsLog(loggerFactory.CreateLogger<JsLog>());
        var databaseProvider = new DatabaseProvider(dalServices);
        var processProvider = new ProcessProvider(loggerFactory.CreateLogger<ProcessProvider>(),
            jsEngineSetup.Value.Api.EnableProcesspApi);
        var environmentProvider = new EnvironmentProvider();

        // This assuming DynamicDependencyAttribute solves the problems with trimming
#pragma warning disable IL2026
#pragma warning disable IL2111
        engine.SetValue("__log", new Action<object?>(val =>
        {
            var value = val?.ToString() ?? "<NULL>";
            System.Diagnostics.Debug.WriteLine(value);
        }));
#pragma warning restore IL2111
#pragma warning restore IL2026


        engine.Modules.Add("dbApi", builder =>
        {
            builder.ExportValue("bucket", context.BucketName);
            builder.ExportObject("http", httpFunctions);
            builder.ExportObject("log", jsLog);
            builder.ExportObject("database", databaseProvider);
            builder.ExportObject("process", processProvider);
            builder.ExportObject("environment", environmentProvider);
            builder.ExportFunction("assert", (args) =>
            {
                if (!args[0].AsBoolean())
                {
                    var errorMessage = args.Length > 1 ? args[1].AsString() : "Assert failed!";
                    throw new Exception(errorMessage); //TODO
                }
            });
        });

        engine.Modules.Add("_main", context.Code);

        if (context.CheckOnly)
        {
            throw new NotImplementedException();
        }

        engine.Modules.Import("_main");
    }
}