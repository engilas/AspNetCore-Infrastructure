using System;
using System.Collections.Concurrent;
using System.Threading;
using Infrastructure.Utils.Async;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Infrastructure.Utils
{
    public class ResponseCacheManager<T>
    {
        private readonly TimeSpan _expireTime = TimeSpan.FromHours(24);

        private readonly ConcurrentDictionary<string, CancellationTokenSource> _groupCts =
            new ConcurrentDictionary<string, CancellationTokenSource>();

        private readonly ILogger _logger;
        private readonly IWlMemoryCache _memoryCache;

        public ResponseCacheManager(ILogger<ResponseCacheManager<T>> logger, ILogger<WlMemoryCache> cacheLogger,
            IOptions<MemoryCacheOptions> memoryCacheOptions)
        {
            _logger = logger;
            //create local instance of memory cache (not global)
            _memoryCache = new WlMemoryCache(new MemoryCache(memoryCacheOptions), cacheLogger);
        }

        public void Add(string group, string key, T response)
        {
            CancellationTokenSource cts = null;
            if (!string.IsNullOrWhiteSpace(group))
            {
                cts = _groupCts.GetOrAdd(group, new CancellationTokenSource());
            }
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _expireTime
            };
            if (cts != null)
            {
                options.ExpirationTokens.Add(new CancellationChangeToken(cts.Token));
            }
            _memoryCache.Set(key, response, options);
        }

        public T Get(string key)
        {
            return _memoryCache.Get<T>(key);
        }

        public void DeleteGroup(string groupName)
        {
            if (!_groupCts.TryRemove(groupName, out var cts))
            {
                _logger.LogError("Can't find group {0}", groupName);
                return;
            }

            cts.Cancel();
            _logger.LogInformation("Removed records for group {0}. Cache count: {1}", groupName, _memoryCache.Count);
        }
    }
}