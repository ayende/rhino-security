using Rhino.Security.Impl;

namespace Rhino.Security
{
    using System;

    /// <summary>
    /// Contains the information about the user that
    /// rhino security requires.
    /// </summary>
    public class SecurityInfo
    {
        private readonly string name;
    	private readonly object identifier;

		/// <summary>
		/// Initializes a new instance of the <see cref="SecurityInfo"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="identifier">The identifier.</param>
        public SecurityInfo(string name, object identifier)
        {
            Guard.Against<ArgumentException>(string.IsNullOrEmpty(name), "Name must have a value");
            Guard.Against<ArgumentException>(identifier==null, "Identifier must not be null");
            this.name = name;
        	this.identifier = identifier;
        }

		/// <summary>
		/// Gets the identifier.
		/// </summary>
		/// <value>The identifier.</value>
    	public virtual object Identifier
    	{
    		get { return identifier; }
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