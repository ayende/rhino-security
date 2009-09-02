namespace Rhino.Security.Tests
{
    public class User : IUser
    {
        public virtual long Id { get; set; }

        public virtual string Name { get; set; }

        /// <summary>
        /// Gets or sets the security info for this user
        /// </summary>
        /// <value>The security info.</value>
        public virtual SecurityInfo SecurityInfo
        {
            get { return new SecurityInfo(Name, Id); }
        }
    }
}