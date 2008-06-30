namespace Rhino.Security.Tests
{
    using System;
    using Castle.ActiveRecord;

    [ActiveRecord(Cache = CacheEnum.ReadWrite)]
    public class User : IUser
    {
        private long id;
        private string name;

        [PrimaryKey]
        public virtual long Id
        {
            get { return id; }
            set { id = value; }
        }

        [Property(NotNull = true)]
        public virtual string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Gets or sets the security info for this user
        /// </summary>
        /// <value>The security info.</value>
        public SecurityInfo SecurityInfo
        {
            get { return new SecurityInfo(name, id); }
        }
    }
}