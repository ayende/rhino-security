using Xunit;

namespace Rhino.Security.Tests
{
    using System;

    public class SecrityInfoFixture
    {
        [Fact]
        public void WillNotAcceptNullName()
        {
            Assert.Throws<ArgumentException>("Name must have a value", () => new SecurityInfo(null, "1"));
        }

        [Fact]
        public void WillNotAcceptEmptyName()
        {
            Assert.Throws<ArgumentException>("Name must have a value", () => new SecurityInfo("", "1"));
        }

        [Fact]
        public void WillNotAcceptNullIdentifier()
        {
            Assert.Throws<ArgumentException>("Identifier must not be null", () => new SecurityInfo("", null));
        }
    }
}