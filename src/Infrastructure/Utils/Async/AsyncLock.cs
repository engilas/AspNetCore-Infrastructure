using System;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Utils.Async
{
    public sealed class AsyncLock : IDisposable
    {
        private readonly Task<IDisposable> m_releaser;
        private readonly SemaphoreSlim m_semaphore = new SemaphoreSlim(1, 1);

        public AsyncLock()
        {
            m_releaser = Task.FromResult((IDisposable) new Releaser(this));
        }

        public void Dispose()
        {
            m_releaser.Dispose();
        }

        public Task<IDisposable> LockAsync()
        {
            var wait = m_semaphore.WaitAsync();
            return wait.IsCompleted
                ? m_releaser
                : wait.ContinueWith((_, state) => (IDisposable) state,
                    m_releaser.Result, CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        private sealed class Releaser : IDisposable
        {
            private readonly AsyncLock m_toRelease;

            internal Releaser(AsyncLock toRelease)
            {
                m_toRelease = toRelease;
            }

            public void Dispose()
            {
                m_toRelease.m_semaphore.Release();
            }
        }
    }
}