using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Utils.Async;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using Shouldly;
using Xunit;

namespace Infrastructure.Tests.Infrastructure
{
    public class WlMemoryCache_Tests
    {
        public WlMemoryCache_Tests()
        {
            _wlMemoryCache = new WlMemoryCache(_cache, NullLogger<WlMemoryCache>.Instance);
        }

        private readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
        private readonly WlMemoryCache _wlMemoryCache;
        private readonly string _key = Guid.NewGuid().ToString();

        private int _callCount;

        private Task<string> ValueFactory()
        {
            ++_callCount;
            return Task.FromResult("a1");
        }

        [Fact]
        public async Task ArgumentValidation_Test()
        {
            Assert.Throws<ArgumentNullException>(() => _wlMemoryCache.Get<object>(null));
            Assert.Throws<ArgumentNullException>(() => _wlMemoryCache.Set(null, "abc"));
            Assert.Throws<ArgumentNullException>(() => _wlMemoryCache.Set(null, "abc", TimeSpan.FromHours(1)));
            Assert.Throws<ArgumentNullException>(() => _wlMemoryCache.Set(null, "abc", null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _wlMemoryCache.GetOrSetAsync(null, ValueFactory));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _wlMemoryCache.GetOrSetAsync<object>(_key, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _wlMemoryCache.GetOrSetAsync(null, TimeSpan.FromSeconds(1), ValueFactory));
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _wlMemoryCache.GetOrSetAsync(null, new MemoryCacheEntryOptions(), ValueFactory));
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _wlMemoryCache.GetOrSetAsync<object>("abc", new MemoryCacheEntryOptions(), null));
        }

        [Fact]
        public void Get_Test()
        {
            _wlMemoryCache.Set(_key, "a1");
            _wlMemoryCache.Get<string>(_key).ShouldBe("a1");
            _cache.Get<string>(_key).ShouldBe("a1");
        }

        [Fact]
        public async Task GetOrSetAsync_Expiration_Test()
        {
            var result = await _wlMemoryCache.GetOrSetAsync(_key, TimeSpan.FromMilliseconds(100), ValueFactory);
            _callCount.ShouldBe(1);
            result.ShouldBe("a1");
            result = _cache.Get<string>(_key);
            result.ShouldBe("a1");
            Thread.Sleep(100);
            _cache.TryGetValue(_key, out result).ShouldBeFalse();
            result = await _wlMemoryCache.GetOrSetAsync(_key, TimeSpan.FromMilliseconds(100), ValueFactory);
            result.ShouldBe("a1");
            _callCount.ShouldBe(2);
        }

        [Fact]
        public async Task GetOrSetAsync_Options_Test()
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddMilliseconds(100)
            };

            var result = await _wlMemoryCache.GetOrSetAsync(_key, options, ValueFactory);
            result.ShouldBe("a1");
            _callCount.ShouldBe(1);
            _cache.TryGetValue(_key, out result).ShouldBeTrue();
            Thread.Sleep(100);
            _cache.TryGetValue(_key, out result).ShouldBeFalse();
            result = await _wlMemoryCache.GetOrSetAsync(_key, options, ValueFactory);
            result.ShouldBe("a1");
            _callCount.ShouldBe(2);
        }

        [Fact]
        public async Task GetOrSetAsync_Test()
        {
            var tasks = new List<Task<string>>();
            for (var i = 0; i < 10; i++) tasks.Add(Task.Run(() => _wlMemoryCache.GetOrSetAsync(_key, ValueFactory)));
            await Task.WhenAll(tasks);
            foreach (var task in tasks) task.Result.ShouldBe("a1");
            _callCount.ShouldBe(1);
        }

        [Fact]
        public void Remove_Test()
        {
            _wlMemoryCache.Set(_key, "a1");
            _cache.TryGetValue(_key, out string result).ShouldBeTrue();
            result.ShouldBe("a1");
            _wlMemoryCache.Remove(_key);
            _cache.TryGetValue(_key, out result).ShouldBeFalse();
        }

        [Fact]
        public void Set_CancelToken_Test()
        {
            var cts = new CancellationTokenSource();
            _wlMemoryCache.Set(_key, "a1",
                new MemoryCacheEntryOptions {ExpirationTokens = {new CancellationChangeToken(cts.Token)}});
            _cache.TryGetValue(_key, out string result).ShouldBeTrue();
            result.ShouldBe("a1");
            cts.Cancel();
            _cache.TryGetValue(_key, out result).ShouldBeFalse();
        }

        [Fact]
        public void Set_Expiration_Test()
        {
            _wlMemoryCache.Set(_key, "a1", TimeSpan.FromMilliseconds(100));
            _cache.TryGetValue(_key, out string result).ShouldBeTrue();
            result.ShouldBe("a1");
            Thread.Sleep(100);
            _cache.TryGetValue(_key, out result).ShouldBeFalse();
        }

        [Fact]
        public void Set_Options_Test()
        {
            _wlMemoryCache.Set(_key, "a1", new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddMilliseconds(100)
            });
            _cache.TryGetValue(_key, out string result).ShouldBeTrue();
            result.ShouldBe("a1");
            Thread.Sleep(100);
            _cache.TryGetValue(_key, out result).ShouldBeFalse();
        }

        [Fact]
        public void Set_Test()
        {
            _wlMemoryCache.Set(_key, "a1");

            _cache.TryGetValue(_key, out string result).ShouldBeTrue();
            result.ShouldBe("a1");
        }
    }
}