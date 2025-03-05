using System.Net;
using DotNext;
using DotNext.Net;
using DotNext.Net.Cluster.Messaging.Gossip;
using DotNext.Net.Http;
using Microsoft.AspNetCore.Connections;

namespace YATsDb.Cluster;

internal sealed class RumorSender(
    IPeerMesh<HttpPeerClient> peerMesh,
    Uri senderAddress,
    RumorTimestamp senderId) : Disposable, IRumorSender
{
    private const string SenderAddressHeader = "X-Sender-Address";
    private const string SenderIdHeader = "X-Rumor-ID";

    internal const string RumorResource = "/rumor";
    internal const string BroadcastResource = "/broadcast";
    internal const string NeighborsResource = "/neighbors";

    private async Task SendAsync(HttpPeerClient client, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, BroadcastResource);

        request.Headers.Add(SenderAddressHeader, senderAddress.ToString());
        request.Headers.Add(SenderIdHeader, senderId.ToString());

        using var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    Task IRumorSender.SendAsync(EndPoint peer, CancellationToken cancellationToken)
    {
        var client = peerMesh.TryGetPeer(peer);
        return client is null || EndPointFormatter.UriEndPointComparer.Equals(new UriEndPoint(senderAddress), peer)
            ? Task.CompletedTask
            : SendAsync(client, cancellationToken);
    }

    internal static Uri ParseSenderAddress(HttpRequest request)
        => new(request.Headers[SenderAddressHeader]!, UriKind.Absolute);

    internal static RumorTimestamp ParseRumorId(HttpRequest request)
        => RumorTimestamp.TryParse(request.Headers[SenderIdHeader], out var result)
            ? result
            : throw new FormatException("Invalid rumor ID");

    public new ValueTask DisposeAsync() => base.DisposeAsync();
}