using Microsoft.Extensions.DependencyInjection;
using YATsDb.Client.Http;
using YATsDb.Core.Services;
using ManagementService = YATsDb.Client.Http.ManagementService;

namespace YATsDb.Client;

public static class Bootstrap
{
    public static void AddYATsDbClient(this IServiceCollection serviceCollection, Settings settings)
    {
        serviceCollection.AddSingleton(settings);
        serviceCollection.AddHttpClient(Settings.ClientName)
            .ConfigureHttpClient(httpClient => httpClient.BaseAddress = new Uri(settings.DbHost));

        serviceCollection.AddScoped<IManagementService, ManagementService>();
        serviceCollection.AddScoped<IDalService, DalService>();
    }
}
