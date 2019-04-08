using System;
using Infrastructure.Extensions;
using Newtonsoft.Json;
using Shouldly;
using Xunit;

namespace Infrastructure.Tests.Infrastructure.Extensions
{
    public class StringExtensions_Tests
    {
        private readonly string _nullString = null;

        private class FromJson_Test_Class
        {
            public int A { get; set; }
            public int B { get; set; }
        }

        [Fact]
        public void EqualsIgnoreCase_Test()
        {
            var a = "abc";
            var b = "Abc";
            var c = "bca";
            var d = "abc";

            a.EqualsIgnoreCase(b).ShouldBeTrue();
            a.EqualsIgnoreCase(c).ShouldBeFalse();
            a.EqualsIgnoreCase(d).ShouldBeTrue();
        }

        [Fact]
        public void FromJson_Test()
        {
            var obj = new FromJson_Test_Class {A = 1, B = 2};
            var json = JsonConvert.SerializeObject(obj);
            var fromObj = json.FromJson<FromJson_Test_Class>();
            fromObj.A.ShouldBe(1);
            fromObj.B.ShouldBe(2);

            var anonObj = new {c = 1, d = 2};
            json = JsonConvert.SerializeObject(anonObj);
            var fromAnonObj = json.FromJson(anonObj);
            fromAnonObj.c.ShouldBe(1);
            fromAnonObj.d.ShouldBe(2);
        }

        [Fact]
        public void GetMd5Hash_Test()
        {
            var input = "abc";
            var md5 = "900150983cd24fb0d6963f7d28e17f72";

            var result = input.GetMd5Hash();
            result.ShouldBe(md5);

            Assert.Throws<ArgumentNullException>(() => _nullString.GetMd5Hash());
        }

        [Fact]
        public void GetSha1Hash_Test()
        {
            var input = "abc";
            var sha1 = "a9993e364706816aba3e25717850c26c9cd0d89d";

            var result = input.GetSha1Hash();
            result.ShouldBe(sha1);

            Assert.Throws<ArgumentNullException>(() => _nullString.GetSha1Hash());
        }

        [Fact]
        public void ReplaceIfNullOrWhitespace_Test()
        {
            var result = "   ".ReplaceIfNullOrWhitespace("a");
            result.ShouldBe("a");
            result = _nullString.ReplaceIfNullOrWhitespace("b");
            result.ShouldBe("b");
            result = "qe".ReplaceIfNullOrWhitespace("c");
            result.ShouldBe("qe");
        }

        [Fact]
        public void ThrowIfNullOrWhitespace_Test()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => _nullString.ThrowIfNullOrWhitespace("test"));
            ex.ParamName.ShouldBe("test");
            Assert.Throws<ArgumentNullException>(() => "abc".ThrowIfNullOrWhitespace(null));
            Assert.Throws<ArgumentNullException>(() => "   ".ThrowIfNullOrWhitespace("test"));
            "abc".ThrowIfNullOrWhitespace("test");
        }

        [Fact]
        public void ToInt32_Test()
        {
            var result = "123".ToInt32();
            result.ShouldBe(123);
            Assert.Throws<ArgumentNullException>(() => _nullString.ToInt32());
            Assert.Throws<FormatException>(() => "1a".ToInt32());
        }

        [Fact]
        public void ToInt64_Test()
        {
            var result = "123".ToInt64();
            result.ShouldBe(123);
            Assert.Throws<ArgumentNullException>(() => _nullString.ToInt64());
            Assert.Throws<FormatException>(() => "1a".ToInt64());
        }

        [Fact]
        public void TryToInt32_Test()
        {
            "123".TryToInt32(out var result).ShouldBeTrue();
            result.ShouldBe(123);
            "1a".TryToInt32(out result).ShouldBeFalse();
        }
    }
}