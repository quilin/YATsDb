using Microsoft.Extensions.Caching.Memory;
using YATsDb.Core.Services;

namespace YATsDB.Server.Services.Implementation;

internal sealed class YatsdbCache : ICache
{
    private readonly IMemoryCache memoryCache;

    public YatsdbCache(IMemoryCache memoryCache)
    {
        this.memoryCache = memoryCache;
    }

    public T GetOrCreate<T>(string cacheKey, Func<(T, TimeSpan)> creationFunction)
    {
        var result = memoryCache.GetOrCreate<T>(cacheKey, entry =>
        {
            var (value, lifeTime) = creationFunction();

            entry.SlidingExpiration = lifeTime;
            return value;
        });

        System.Diagnostics.Debug.Assert(result is not null);
        return result;
    }
}