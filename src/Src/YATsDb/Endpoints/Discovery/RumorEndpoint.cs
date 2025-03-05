using DotNext.Net;
using DotNext.Net.Cluster.Discovery.HyParView;
using DotNext.Net.Cluster.Discovery.HyParView.Http;
using DotNext.Net.Cluster.Messaging.Gossip;
using DotNext.Net.Http;
using Microsoft.Extensions.Options;
using YATsDb.Cluster;

namespace YATsDb.Endpoints.Discovery;

internal static class RumorEndpoint
{
    public static void AddRumorEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(RumorSender.RumorResource, context =>
        {
            var (sender, id) = PrepareMessageId(context.RequestServices);
            return context.RequestServices.GetRequiredService<PeerController>()
                .EnqueueBroadcastAsync(controller =>
                    new RumorSender((IPeerMesh<HttpPeerClient>) controller, sender, id))
                .AsTask();
        });
    }

    private static (Uri, RumorTimestamp) PrepareMessageId(IServiceProvider services)
    {
        var config = services.GetRequiredService<IOptions<HttpPeerConfiguration>>().Value;
        var manager = services.GetRequiredService<RumorSpreadingManager>();
        return (config.LocalNode!, manager.Tick());
    }
}