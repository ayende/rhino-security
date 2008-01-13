namespace Rhino.Security
{
    using System;
    using Commons;

    /// <summary>
    /// Contains the information about the user that
    /// rhino security requires.
    /// </summary>
    public class SecurityInfo
    {
        private readonly string name;
        private readonly Guid securityKey;
        private readonly string securityKeyPropertyName;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityInfo"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="securityId">The securityKey.</param>
        /// <param name="securityIdPropertyName">Name of the security id property.</param>
        public SecurityInfo(string name, Guid securityId, string securityIdPropertyName)
        {
            Guard.Against<ArgumentException>(string.IsNullOrEmpty(name), "Name must have a value");
            Guard.Against<ArgumentException>(securityId==Guid.Empty, "SecurityKey must not be an empty guid");
            Guard.Against<ArgumentException>(string.IsNullOrEmpty(securityIdPropertyName), "SecurityKeyPropertyName must have a value");
            this.name = name;
            this.securityKeyPropertyName = securityIdPropertyName;
            this.securityKey = securityId;
        }

        /// <summary>
        /// Gets the name of the security id property.
        /// </summary>
        /// <value>The name of the security id property.</value>
        public string SecurityKeyPropertyName
        {
            get { return securityKeyPropertyName; }
        }

        /// <summary>
        /// Gets the security id.
        /// </summary>
        /// <value>The security id.</value>
        public virtual Guid SecurityKey
        {
            get { return securityKey; }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public virtual string Name
        {
            get { return name; }
        }
    }
}