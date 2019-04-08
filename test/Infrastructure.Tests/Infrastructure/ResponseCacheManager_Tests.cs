using Infrastructure.Utils;
using Infrastructure.Utils.Async;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using Xunit;

namespace Infrastructure.Tests.Infrastructure
{
    public class ResponseCacheManager_Tests
    {
        public ResponseCacheManager_Tests()
        {
            _cacheManager = new ResponseCacheManager<int>(NullLogger<ResponseCacheManager<int>>.Instance,
                NullLogger<WlMemoryCache>.Instance, new MemoryCacheOptions());
        }

        private readonly ResponseCacheManager<int> _cacheManager;

        [Fact]
        public void Add_Get_Test()
        {
            _cacheManager.Add("g1", "k1", 50);
            _cacheManager.Get("k1").ShouldBe(50);
            _cacheManager.Get("k2").ShouldBe(default(int));
        }

        [Fact]
        public void DeleteGroup_Test()
        {
            _cacheManager.Add("g1", "k1", 50);
            _cacheManager.Add("g1", "k2", 51);
            _cacheManager.Add("g1", "k3", 52);
            _cacheManager.Get("k1").ShouldBe(50);
            _cacheManager.DeleteGroup("g1");

            _cacheManager.Get("k1").ShouldBe(default(int));
            _cacheManager.Get("k2").ShouldBe(default(int));
            _cacheManager.Get("k3").ShouldBe(default(int));
        }
    }
}