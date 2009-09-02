namespace Rhino.Security.Tests
{
    using System;

    public class Account
    {
        public Account()
        {
            SecurityKey = Guid.NewGuid();
        }

        public virtual long Id { get; set; }

        public virtual Guid SecurityKey { get; set; }

        public virtual string Name { get; set; }
    }
}