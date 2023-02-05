using System;
using System.Threading.Tasks;
using AsyncKeyedLock;
using Infrastructure.Extensions;
using Infrastructure.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Utils.Async
{
    /// <summary>
    ///     Async memory cache, based on <see cref="IMemoryCache" />. Supports key-based locks.
    /// </summary>
    public class WlMemoryCache : IWlMemoryCache
    {
        private readonly AsyncKeyedLocker<object> _asyncLock = new(o =>
        {
            o.PoolSize = 20;
            o.PoolInitialFill = 1;
        });
        private readonly TimeSpan _defaultExpiration = TimeSpan.FromHours(1);
        private readonly ILogger<WlMemoryCache> _logger;
        private readonly MemoryCache _memoryCache;

        public WlMemoryCache(IMemoryCache memoryCache, ILogger<WlMemoryCache> logger)
        {
            _logger = logger;
            _memoryCache = (MemoryCache) memoryCache;
        }

        public int Count => _memoryCache.Count;

        public T Get<T>(object key)
        {
            //todo: test for invalid cast
            if (_memoryCache.TryGetValue(key, out T value)) return value;
            return default;
        }

        public void Set<T>(object key, T value)
        {
            Set(key, value, _defaultExpiration);
        }

        public void Set<T>(object key, T value, TimeSpan expiration)
        {
            Set(key, value, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            });
        }

        public void Set<T>(object key, T value, MemoryCacheEntryOptions options)
        {
            options.ThrowIfNullArgument(nameof(options));
            options.PostEvictionCallbacks.Add(
                new PostEvictionCallbackRegistration {EvictionCallback = EvictionCallback});
            _memoryCache.Set(key, value, options);
        }

        public Task<T> GetOrSetAsync<T>(object key, Func<Task<T>> factory)
        {
            return GetOrSetAsync(key, _defaultExpiration, factory);
        }

        public Task<T> GetOrSetAsync<T>(object key, TimeSpan expiration, Func<Task<T>> factory)
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };
            return GetOrSetAsync(key, options, factory);
        }

        public async Task<T> GetOrSetAsync<T>(object key, MemoryCacheEntryOptions options, Func<Task<T>> factory)
        {
            factory.ThrowIfNullArgument(nameof(factory));
            if (!_memoryCache.TryGetValue(key, out T value))
                using (await _asyncLock.LockAsync(key).ConfigureAwait(false))
                {
                    if (!_memoryCache.TryGetValue(key, out value))
                    {
                        value = await factory();
                        Set(key, value, options);
                    }
                }

            return value;
        }

        public void Remove(object key)
        {
            _memoryCache.Remove(key);
        }

        private void EvictionCallback(object key, object value, EvictionReason reason, object state)
        {
            _logger.LogDebugItems("Eviction was processed", new {cacheCount = Count, key, value, reason});
        }
    }
}