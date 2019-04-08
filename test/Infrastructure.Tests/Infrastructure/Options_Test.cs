using System;
using Infrastructure.Configuration;
using Infrastructure.Exceptions;
using Xunit;

namespace Infrastructure.Tests.Infrastructure
{
    public class Options_Test
    {
        private class Options : OptionsBase
        {
            public override void Validate()
            {
            }

            public new void ValidateEmptyString(string parameterName, string value)
            {
                base.ValidateEmptyString(parameterName, value);
            }

            public new void ValidateNegativeValue(string parameterName, int value)
            {
                base.ValidateNegativeValue(parameterName, value);
            }

            public new void ValidateNotPositiveValue(string parameterName, int value)
            {
                base.ValidateNotPositiveValue(parameterName, value);
            }
        }

        [Fact]
        public void ValidateEmptyString_Test()
        {
            var n = "test";
            var opts = new Options();
            opts.ValidateEmptyString(n, "b");
            Assert.Throws<OptionsValidationException>(() => opts.ValidateEmptyString(n, " "));
            Assert.Throws<OptionsValidationException>(() => opts.ValidateEmptyString(n, null));
            Assert.Throws<ArgumentNullException>(() => opts.ValidateEmptyString(null, "b"));
        }

        [Fact]
        public void ValidateNegativeValue_Test()
        {
            var n = "test";
            var opts = new Options();
            opts.ValidateNegativeValue(n, 1);
            opts.ValidateNegativeValue(n, 0);
            Assert.Throws<OptionsValidationException>(() => opts.ValidateNegativeValue(n, -1));
            Assert.Throws<ArgumentNullException>(() => opts.ValidateNegativeValue(null, 4));
        }

        [Fact]
        public void ValidateNotPositiveValue_Test()
        {
            var n = "test";
            var opts = new Options();
            opts.ValidateNotPositiveValue(n, 1);
            Assert.Throws<OptionsValidationException>(() => opts.ValidateNotPositiveValue(n, 0));
            Assert.Throws<OptionsValidationException>(() => opts.ValidateNotPositiveValue(n, -1));
            Assert.Throws<ArgumentNullException>(() => opts.ValidateNotPositiveValue(null, 4));
        }
    }
}