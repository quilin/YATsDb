using DotNext.Net;
using DotNext.Net.Cluster.Discovery.HyParView;
using DotNext.Net.Cluster.Messaging.Gossip;
using DotNext.Net.Http;
using Microsoft.AspNetCore.Connections;
using YATsDb.Cluster;

namespace YATsDb.Endpoints.Discovery;

internal static class BroadcastEndpoint
{
    public static void AddBroadcastEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(RumorSender.BroadcastResource, context =>
        {
            var sender = RumorSender.ParseSenderAddress(context.Request);
            var id = RumorSender.ParseRumorId(context.Request);

            var spreadingManager = context.RequestServices.GetRequiredService<RumorSpreadingManager>();
            if (!spreadingManager.CheckOrder(new UriEndPoint(sender), id)) return Task.CompletedTask;

            context.RequestServices.GetRequiredService<ILogger>()
                .LogTrace("Spreading rumor from {Sender} with sequence number = {Id}", sender, id);

            return context.RequestServices.GetRequiredService<PeerController>()
                .EnqueueBroadcastAsync(
                    controller => new RumorSender((IPeerMesh<HttpPeerClient>) controller, sender, id))
                .AsTask();
        });
    }
}