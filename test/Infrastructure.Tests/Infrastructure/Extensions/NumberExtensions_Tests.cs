using System;
using Infrastructure.Extensions;
using Shouldly;
using Xunit;

namespace Infrastructure.Tests.Infrastructure.Extensions
{
    public class NumberExtensions_Tests
    {
        [Fact]
        public void In_Test()
        {
            var range = (from: 5, to: 10);

            6.In(range).ShouldBeTrue();
            6.In(range.from, range.to).ShouldBeTrue();

            5.In(range).ShouldBeTrue();
            5.In(range.from, range.to).ShouldBeTrue();

            4.In(range).ShouldBeFalse();
            4.In(range.from, range.to).ShouldBeFalse();

            10.In(range).ShouldBeTrue();
            10.In(range.from, range.to).ShouldBeTrue();

            11.In(range).ShouldBeFalse();
            11.In(range.from, range.to).ShouldBeFalse();

            range = (from: 10, to: 5);

            Assert.Throws<ArgumentException>(() => 6.In(range));
        }
    }
}