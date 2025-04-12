using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using NCronJob;
using Serilog;
using Tenray.ZoneTree;
using YATsDB.Server;
using YATsDB.Server.Endpoints;
using YATsDB.Server.Endpoints.Common;
using YATsDB.Server.Infrastructure.Workers;
using YATsDB.Server.Services.Configuration;
using YATsDB.Server.Services.Contracts;
using YATsDB.Server.Services.Implementation;
using YATsDB.Server.Services.Implementation.JsEngine;

var builder = WebApplication.CreateSlimBuilder(args);
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();

builder.Services.AddOpenApiDocument();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddValidators();
builder.Services.AddProblemDetails();

// TODO: Find a way to validate it properly
#pragma warning disable IL2026
builder.Services
    .AddOptions<DbSetup>()
    .BindConfiguration("DbSetup")
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<JsEngineSetup>()
    .BindConfiguration("JsEngineSetup")
    .ValidateDataAnnotations()
    .ValidateOnStart();
#pragma warning restore IL2026

builder.Services.AddTransient<YATsDb.Core.Services.IManagementService, YATsDb.Core.Services.ManagementService>();
builder.Services.AddTransient<YATsDb.Core.Services.IDalService, YATsDb.Core.Services.DalServices>();
builder.Services.AddTransient<ICronManagement, CronManagement>();
builder.Services.AddTransient<IJsInternalEngine, JsInternalEngine>();

builder.Services
    .AddTransient<YATsDb.Core.HighLevel.IYatsdbHighLevelStorage, YATsDb.Core.HighLevel.YatsdbHighLevelStorage>();
builder.Services
    .AddTransient<YATsDb.Core.LowLevel.IYatsdbLowLevelStorage, YATsDb.Core.LowLevel.YatsdbLowLevelStorage>();
builder.Services.AddTransient<YATsDb.Core.LowLevel.IKvStorage, YATsDb.Core.LowLevel.KvStorage>();
builder.Services.AddSingleton<IZoneTree<Memory<byte>, Memory<byte>>>(sp =>
{
    var dbSetup = sp.GetRequiredService<IOptions<DbSetup>>().Value;

    return YATsDb.Core.ZoneTreeFactory.Build(factory =>
    {
        factory.SetDataDirectory(dbSetup.DbPath);
        factory.ConfigureWriteAheadLogOptions(opt => { opt.WriteAheadLogMode = dbSetup.WriteAheadLogMode; });

        if (dbSetup.MaxMutableSegmentsCount > 0)
        {
            factory.SetMutableSegmentMaxItemCount(dbSetup.MaxMutableSegmentsCount.Value);
        }
    });
});

builder.Services.AddHostedService<ZoneTreeMaintainerHostedService<Memory<byte>, Memory<byte>>>();
builder.Services.AddTransient<YATsDb.Core.Services.ICache, YatsdbCache>();

builder.Services.AddNCronJob(cfg =>
{
    cfg.AddJob<CronTriggerJob>();
    cfg.AddJob<CronStartupRegisterJob>().RunAtStartup();
});

var app = builder.Build();

app.UseForwardedHeaders();

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        await Results.Problem()
            .ExecuteAsync(context);
    });
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    // app.UseHsts();
}
else
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

app.UseHttpsRedirection();
app.AddAppEndpoints();

app.Run();

[JsonSerializable(typeof(CronPostEndpoint.CreateCronDto))]
[JsonSerializable(typeof(ManagementPostBucketsEndpoint.CreateBucketDto))]
[JsonSerializable(typeof(List<ManagementGetBucketsEndpoint.BucketInfoDto>))]
[JsonSerializable(typeof(PostQueryEndpoint.QueryDal))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(CronJobData))]
[JsonSerializable(typeof(Dictionary<string, object>))]
[JsonSerializable(typeof(QueryResult))]
[JsonSerializable(typeof(double))]
internal sealed partial class AppJsonSerializerContext : JsonSerializerContext
{
}
