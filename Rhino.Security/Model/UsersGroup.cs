using System.Collections.Generic;

namespace Rhino.Security.Model
{
	/// <summary>
	/// A named group for users, which we 
	/// can define operations on.
	/// </summary>
	public class UsersGroup : NamedEntity<UsersGroup>
	{
	    ///<summary>
        /// Create a new instance of <c>UsersGroup</c>
	    ///</summary>
	    public UsersGroup()
	    {
	        Users = new HashSet<IUser>();
	        AllParents = new HashSet<UsersGroup>();
	        AllChildren = new HashSet<UsersGroup>();
	        DirectChildren = new HashSet<UsersGroup>();
	    }

	    /// <summary>
	    /// Gets or sets the users.
	    /// </summary>
	    /// <value>The users.</value>
	    public virtual ICollection<IUser> Users { get; set; }

	    /// <summary>
	    /// Gets or sets the parent of this group
	    /// </summary>
	    /// <value>The parent.</value>
	    public virtual UsersGroup Parent { get; set; }

	    /// <summary>
	    /// Gets or sets the direct children of this group (nested one level)
	    /// </summary>
	    /// <value>The directChildren.</value>
	    public virtual ICollection<UsersGroup> DirectChildren { get; set; }


	    /// <summary>
	    /// Gets or sets all children of this users group, at all nesting levels
	    /// </summary>
	    /// <value>All children.</value>
	    public virtual ICollection<UsersGroup> AllChildren { get; set; }


	    /// <summary>
	    /// Gets or sets all parent of this users group, at all nesting levels
	    /// </summary>
	    /// <value>All children.</value>
	    public virtual ICollection<UsersGroup> AllParents { get; set; }
	}
}