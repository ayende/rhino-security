namespace Rhino.Security.Tests
{
    using System;
    using MbUnit.Framework;

    [TestFixture]
    public class SecrityInfoFixture
    {
        [Test]
        [ExpectedException(typeof(ArgumentException), "SecurityKey must not be an empty guid")]
        public void WillNotAcceptEmptyGuid()
        {
            new SecurityInfo("2", Guid.Empty, "1");
        }

        [Test]
        [ExpectedException(typeof(ArgumentException),"Name must have a value")]
        public void WillNotAcceptNullName()
        {
            new SecurityInfo(null, Guid.NewGuid(), "1");
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), "Name must have a value")]
        public void WillNotAcceptEmptyName()
        {
            new SecurityInfo("", Guid.NewGuid(), "1");
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), "SecurityKeyPropertyName must have a value")]
        public void WillNotAcceptNullSecurityPropertyId()
        {
            new SecurityInfo("a", Guid.NewGuid(), null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), "SecurityKeyPropertyName must have a value")]
        public void WillNotAcceptEmptySecurityPropertyId()
        {
            new SecurityInfo("a", Guid.NewGuid(), "");
        }
    }
}