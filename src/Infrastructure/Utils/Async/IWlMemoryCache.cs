using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Utils.Async
{
    /// <summary>
    ///     Async memory cache, based on <see cref="IMemoryCache" />. Supports key-based locks.
    /// </summary>
    public interface IWlMemoryCache
    {
        int Count { get; }
        T Get<T>(object key);
        void Set<T>(object key, T value);
        void Set<T>(object key, T value, TimeSpan expiration);
        void Set<T>(object key, T value, MemoryCacheEntryOptions options);
        Task<T> GetOrSetAsync<T>(object key, Func<Task<T>> factory);
        Task<T> GetOrSetAsync<T>(object key, TimeSpan expiration, Func<Task<T>> factory);
        Task<T> GetOrSetAsync<T>(object key, MemoryCacheEntryOptions options, Func<Task<T>> factory);
        void Remove(object key);
    }
}