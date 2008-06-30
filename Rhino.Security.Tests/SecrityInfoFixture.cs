namespace Rhino.Security.Tests
{
    using System;
    using MbUnit.Framework;

    [TestFixture]
    public class SecrityInfoFixture
    {
        [Test]
        [ExpectedException(typeof(ArgumentException),"Name must have a value")]
        public void WillNotAcceptNullName()
        {
            new SecurityInfo(null, "1");
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), "Name must have a value")]
        public void WillNotAcceptEmptyName()
        {
            new SecurityInfo("",  "1");
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), "Identifier must not be null")]
        public void WillNotAcceptNullIdentifier()
        {
            new SecurityInfo("a", null);
        }
    }
}