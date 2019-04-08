using System;
using Infrastructure.Extensions;
using Newtonsoft.Json;
using Shouldly;
using Xunit;

namespace Infrastructure.Tests.Infrastructure.Extensions
{
    public class CommonExtensions_Tests
    {
        [Fact]
        public void CastTo_Test()
        {
            object a = 5;
            var b = a.CastTo<int>();
            b.ShouldBe(5);
            Assert.Throws<InvalidCastException>(() => a.CastTo<long>());
        }

        [Fact]
        public void If_Test()
        {
            var affected1 = false;
            var affected2 = false;

            var target = new object();

            target.If(true, x =>
            {
                affected1 = true;
                return x;
            });

            target.If(false, x =>
            {
                affected2 = true;
                return x;
            });

            affected1.ShouldBeTrue();
            affected2.ShouldBeFalse();
        }

        [Fact]
        public void In_Test()
        {
            var objects = new[] {new object(), new object(), new object()};

            objects[1].In(objects).ShouldBeTrue();
            new object().In(objects).ShouldBeFalse();

            var set = new[] {1, 2, 3};
            2.In(set).ShouldBeTrue();
            0.In(set).ShouldBeFalse();

            Assert.Throws<ArgumentNullException>(() => ((object) null).In(objects));
            Assert.Throws<ArgumentNullException>(() => 5.In(null));
        }

        [Fact]
        public void Map_Test()
        {
            Assert.Throws<ArgumentNullException>(() => ((object) null).Map(x => x));
            Assert.Throws<ArgumentNullException>(() => new object().Map((Func<object, object>) null));

            var obj = new {a = 1, b = 2};
            var mappedObj = obj.Map(o => new {c = o.a, d = o.b});
            mappedObj.c.ShouldBe(obj.a);
            mappedObj.d.ShouldBe(obj.b);
        }

        [Fact]
        public void ThrowIfNull_Test()
        {
            object nullObj = null;
            var ex = Assert.Throws<NullReferenceException>(() => nullObj.ThrowIfNull("test"));
            ex.Message.ShouldBe("test");
            new object().ThrowIfNull("test");
            Assert.Throws<ArgumentNullException>(() => new object().ThrowIfNull(null));
        }

        [Fact]
        public void ThrowIfNullArgument_Test()
        {
            object nullObj = null;
            var ex = Assert.Throws<ArgumentNullException>(() => nullObj.ThrowIfNullArgument("test"));
            ex.ParamName.ShouldBe("test");
            new object().ThrowIfNullArgument("test");
            Assert.Throws<ArgumentNullException>(() => new object().ThrowIfNullArgument(null));
        }

        [Fact]
        public void ToJson_Test()
        {
            object nullObj = null;
            Assert.Throws<ArgumentNullException>(() => nullObj.ToJson());
            var obj = new {a = 1, b = 2};
            var json = obj.ToJson();
            var fromJson = JsonConvert.DeserializeAnonymousType(json, obj);
            fromJson.a.ShouldBe(1);
            fromJson.b.ShouldBe(2);
        }
    }
}