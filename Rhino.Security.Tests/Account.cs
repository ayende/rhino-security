namespace Rhino.Security.Tests
{
    using System;
    using Castle.ActiveRecord;

    [ActiveRecord]
    public class Account
    {
        private long id;
        private Guid securityKey = Guid.NewGuid();
        private string name;

        [PrimaryKey]
        public virtual long Id
        {
            get { return id; }
            set { id = value; }
        }

        [Property(NotNull = true)]
        public virtual Guid SecurityKey
        {
            get { return securityKey; }
            set { securityKey = value; }
        }

        [Property(NotNull = true)]
        public virtual string Name
        {
            get { return name; }
            set { name = value; }
        }
    }
}