using AsyncKeyedLock;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Infrastructure.Tests.Infrastructure
{
    public class AsyncKeyLock_Tests
    {
        private readonly AsyncKeyedLocker<string> _keyLock = new AsyncKeyedLocker<string>(o =>
        {
            o.PoolSize = 20;
            o.PoolInitialFill = 1;
        });

        private class Container
        {
            private bool _init;
            private int _initCount;

            public void Initialize()
            {
                _init = true;
                ++_initCount;
            }

            public bool IsInitialized()
            {
                return _init;
            }

            public void CheckCorrectInitCount()
            {
                _initCount.ShouldBe(1);
            }
        }

        [Fact]
        public async Task LockAsync_Test()
        {
            var tasks = new List<Task>();

            var key1 = Guid.NewGuid().ToString();
            var container1 = new Container();
            var key2 = Guid.NewGuid().ToString();
            var container2 = new Container();

            for (var i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    using (await _keyLock.LockAsync(key1).ConfigureAwait(false))
                    {
                        if (container1.IsInitialized()) return;
                        container1.Initialize();
                    }
                }));
                tasks.Add(Task.Run(async () =>
                {
                    using (await _keyLock.LockAsync(key2).ConfigureAwait(false))
                    {
                        if (container2.IsInitialized()) return;
                        container2.Initialize();
                    }
                }));
            }

            await Task.WhenAll(tasks);

            container1.CheckCorrectInitCount();
            container2.CheckCorrectInitCount();
        }
    }
}