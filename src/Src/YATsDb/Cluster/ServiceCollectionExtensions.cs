using DotNext.Net;
using DotNext.Net.Cluster.Discovery.HyParView;
using DotNext.Net.Cluster.Messaging.Gossip;

namespace YATsDb.Cluster;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRaftCluster(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddHyParViewDiscovery();
    }

    private static IServiceCollection AddHyParViewDiscovery(
        this IServiceCollection services)
    {
        return services
            .AddSingleton(new RumorSpreadingManager(EndPointFormatter.UriEndPointComparer))
            .AddSingleton<IPeerLifetime, HyParViewPeerLifetime>()
            .AddSingleton<IHttpMessageHandlerFactory, HyParViewClientHandlerFactory>();
    }
}