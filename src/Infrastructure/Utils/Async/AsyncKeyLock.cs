using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Utils.Async
{
    /// <summary>
    ///     Async key-based thread synchronization primitive
    /// </summary>
    public sealed class AsyncKeyLock
    {
        /// <summary>
        ///     For global lock
        /// </summary>
        private static readonly Dictionary<object, RefCounted<SemaphoreSlim>> SemaphoreSlimsStatic
            = new Dictionary<object, RefCounted<SemaphoreSlim>>();

        private readonly Dictionary<object, RefCounted<SemaphoreSlim>> SemaphoreSlims;

        /// <summary>
        ///     For local lock
        /// </summary>
        private readonly Dictionary<object, RefCounted<SemaphoreSlim>> SemaphoreSlimsInstance
            = new Dictionary<object, RefCounted<SemaphoreSlim>>();

        public AsyncKeyLock(bool globalLock = false)
        {
            if (globalLock)
                SemaphoreSlims = SemaphoreSlimsStatic;
            else
                SemaphoreSlims = SemaphoreSlimsInstance;
        }

        private SemaphoreSlim GetOrCreate(object key)
        {
            RefCounted<SemaphoreSlim> item;
            lock (SemaphoreSlims)
            {
                if (SemaphoreSlims.TryGetValue(key, out item))
                {
                    ++item.RefCount;
                }
                else
                {
                    item = new RefCounted<SemaphoreSlim>(new SemaphoreSlim(1, 1));
                    SemaphoreSlims[key] = item;
                }
            }

            return item.Value;
        }

        public IDisposable Lock(object key)
        {
            GetOrCreate(key).Wait();
            return new Releaser(SemaphoreSlims) {Key = key};
        }

        /// <summary>
        ///     Async lock for threads using same key parameter
        /// </summary>
        /// <param name="key">Async lock by given key object</param>
        /// <returns></returns>
        public async Task<IDisposable> LockAsync(object key)
        {
            await GetOrCreate(key).WaitAsync().ConfigureAwait(false);
            return new Releaser(SemaphoreSlims) {Key = key};
        }

        private sealed class RefCounted<T>
        {
            public RefCounted(T value)
            {
                RefCount = 1;
                Value = value;
            }

            public int RefCount { get; set; }
            public T Value { get; }
        }

        private sealed class Releaser : IDisposable
        {
            private readonly Dictionary<object, RefCounted<SemaphoreSlim>> _semaphoreSlims;

            public Releaser(Dictionary<object, RefCounted<SemaphoreSlim>> semaphoreSlims)
            {
                _semaphoreSlims = semaphoreSlims;
            }

            public object Key { get; set; }

            public void Dispose()
            {
                RefCounted<SemaphoreSlim> item;
                lock (_semaphoreSlims)
                {
                    item = _semaphoreSlims[Key];
                    --item.RefCount;
                    if (item.RefCount == 0) _semaphoreSlims.Remove(Key);
                }

                item.Value.Release();
            }
        }
    }
}