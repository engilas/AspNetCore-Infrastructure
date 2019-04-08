using System;
using Infrastructure.Extensions;
using Shouldly;
using Xunit;

namespace Infrastructure.Tests.Infrastructure.Extensions
{
    public class DateTimeExtensions_Test
    {
        [Fact]
        public void UtcToUnixTimeMilliseconds_Test()
        {
            var dt = new DateTime(2000, 1, 1, 5, 5, 51);
            dt.UtcToUnixTimeMilliseconds().ShouldBe(946703151000);
        }

        [Fact]
        public void UtcToUnixTimeSeconds_Test()
        {
            var dt = new DateTime(2000, 1, 1);
            dt.UtcToUnixTimeSeconds().ShouldBe(946684800);
        }
    }
}