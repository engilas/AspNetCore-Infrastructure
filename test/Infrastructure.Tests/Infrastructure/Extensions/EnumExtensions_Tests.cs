using System;
using Infrastructure.Extensions;
using Shouldly;
using Xunit;

namespace Infrastructure.Tests.Infrastructure.Extensions
{
    public class EnumExtensions_Tests
    {
        private readonly string _nullString = null;

        private enum TestEnum
        {
            A,
            B,
            C
        }

        private enum TestEnum1
        {
            A,
            B,
            C,
            D
        }

        [Fact]
        public void Convert_Test()
        {
            TestEnum1? nullEnum = null;

            TestEnum1.B.Convert<TestEnum>().ShouldBe(TestEnum.B);
            Assert.Throws<ArgumentNullException>(() => nullEnum.Convert<TestEnum>());
            Assert.Throws<ArgumentException>(() => TestEnum1.D.Convert<TestEnum>());
        }

        [Fact]
        public void Parse_Test()
        {
            "B".Parse<TestEnum>().ShouldBe(TestEnum.B);
            "b".Parse<TestEnum>().ShouldBe(TestEnum.B);
            Assert.Throws<ArgumentException>(() => "g".Parse<TestEnum>());
            Assert.Throws<ArgumentNullException>(() => _nullString.Parse<TestEnum>());
        }
    }
}