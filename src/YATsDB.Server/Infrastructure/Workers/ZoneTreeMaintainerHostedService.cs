using Tenray.ZoneTree;
using Tenray.ZoneTree.Core;

namespace YATsDB.Server.Infrastructure.Workers;

//https://github.com/koculu/ZoneTree/discussions/53
public sealed class ZoneTreeMaintainerHostedService<TKey, TValue> : IHostedService, IDisposable
{
    private readonly ZoneTreeMaintainer<TKey, TValue> zoneTreeMaintainer;

    public ZoneTreeMaintainerHostedService(IZoneTree<TKey, TValue> zoneTree)
    {
        zoneTreeMaintainer = new ZoneTreeMaintainer<TKey, TValue>(zoneTree, false);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        zoneTreeMaintainer.EnableJobForCleaningInactiveCaches = true;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        zoneTreeMaintainer.EnableJobForCleaningInactiveCaches = false;
        return zoneTreeMaintainer.WaitForBackgroundThreadsAsync();
    }

    public void Dispose()
    {
        zoneTreeMaintainer?.Dispose();
    }
}
