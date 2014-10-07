using Xunit;

namespace Rhino.Security.Tests
{
    using System;

    public class SecrityInfoFixture
    {
        [Fact]
        public void WillNotAcceptNullName()
        {
            // Name must have a value
            Assert.Throws<ArgumentException>(() => new SecurityInfo(null, "1"));
        }

        [Fact]
        public void WillNotAcceptEmptyName()
        {
            // Name must have a value
            Assert.Throws<ArgumentException>(() => new SecurityInfo("", "1"));
        }

        [Fact]
        public void WillNotAcceptNullIdentifier()
        {
            // Identifier must not be null
            Assert.Throws<ArgumentException>(() => new SecurityInfo("", null));
        }
    }
}