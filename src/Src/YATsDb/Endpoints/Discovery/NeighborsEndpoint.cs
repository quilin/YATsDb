using System.Text;
using DotNext.Net;
using DotNext.Net.Http;
using YATsDb.Cluster;

namespace YATsDb.Endpoints.Discovery;

internal static class NeighborsEndpoint
{
    public static void AddNeighborsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(RumorSender.NeighborsResource, context =>
        {
            var peerMesh = context.RequestServices.GetRequiredService<IPeerMesh<HttpPeerClient>>();
            var responseText = peerMesh.Peers.Aggregate(new StringBuilder(), (sb, peer) =>
                sb.AppendLine(peer.ToString())).ToString();
            return context.Response.WriteAsync(responseText, context.RequestAborted);
        });
    }
}