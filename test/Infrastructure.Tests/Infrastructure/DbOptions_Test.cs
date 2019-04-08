using System.Collections.Generic;
using Infrastructure.Repository;
using Shouldly;
using Xunit;

namespace Infrastructure.Tests.Infrastructure
{
    public class DbOptions_Test
    {
        [Fact]
        public void GetCorrectConnectionString_Test()
        {
            var opts = new DbOptions
            {
                ConnectionStrings = new Dictionary<string, string> {{"default", "1"}, {"custom", "3"}}
            };
            opts["custom"].ShouldBe("3");
            opts.GetConnectionString().ShouldBe("1");
            Assert.Throws<KeyNotFoundException>(() => opts["asdf"]);
            Assert.Throws<KeyNotFoundException>(() => opts.GetConnectionString("qwe"));
        }
    }
}