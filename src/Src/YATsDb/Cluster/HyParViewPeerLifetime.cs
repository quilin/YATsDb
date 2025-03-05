using DotNext.Net;
using DotNext.Net.Cluster.Discovery.HyParView;
using DotNext.Net.Cluster.Messaging.Gossip;

namespace YATsDb.Cluster;

internal sealed class HyParViewPeerLifetime(
    RumorSpreadingManager spreadingManager,
    ILogger<HyParViewPeerLifetime> logger) : IPeerLifetime
{
    private void OnPeerDiscovered(PeerController controller, PeerEventArgs args)
    {
        logger.LogTrace("Peer {PeerAddress} has been discovered by the current node", args.PeerAddress.ToString());
        spreadingManager.TryEnableControl(args.PeerAddress);
    }

    private void OnPeerGone(PeerController controller, PeerEventArgs args)
    {
        logger.LogTrace("Peer {PeerAddress} is no longer visible by the current node", args.PeerAddress.ToString());
        spreadingManager.TryDisableControl(args.PeerAddress);
    }

    void IPeerLifetime.OnStart(PeerController controller)
    {
        controller.PeerDiscovered += OnPeerDiscovered;
        controller.PeerGone += OnPeerGone;
    }

    void IPeerLifetime.OnStop(PeerController controller)
    {
        controller.PeerDiscovered -= OnPeerDiscovered;
        controller.PeerGone -= OnPeerGone;
    }
}