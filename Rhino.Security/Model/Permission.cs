using System;
using Rhino.Security.Model;

namespace Rhino.Security.Model
{
	/// <summary>
	/// Represent a permission on the system, allow (or denying) 
	/// [operation] for [someone] on [something]
	/// </summary>
	public class Permission : EqualityAndHashCodeProvider<Permission>
	{
	    /// <summary>
	    /// Gets or sets the operation this permission applies to
	    /// </summary>
	    /// <value>The operation.</value>
	    public virtual Operation Operation { get; set; }

	    /// <summary>
	    /// Gets or sets the entity security key this permission belongs to
	    /// </summary>
	    /// <value>The entity security key.</value>
	    public virtual Guid? EntitySecurityKey { get; set; }

	    /// <summary>
	    /// Gets or sets a value indicating whether this <see cref="Permission"/> is allowing 
	    /// or denying the operation.
	    /// </summary>
	    /// <value><c>true</c> if allow; otherwise, <c>false</c>.</value>
	    public virtual bool Allow { get; set; }

	    /// <summary>
	    /// Gets or sets the user this permission belongs to.
	    /// </summary>
	    /// <value>The user.</value>
	    public virtual IUser User { get; set; }

	    /// <summary>
	    /// Gets or sets the users group this permission belongs to
	    /// </summary>
	    /// <value>The users group.</value>
	    public virtual UsersGroup UsersGroup { get; set; }


	    /// <summary>
	    /// Gets or sets the entities group this permission belongs to
	    /// </summary>
	    /// <value>The entities group.</value>
	    public virtual EntitiesGroup EntitiesGroup { get; set; }

	    /// <summary>
	    /// Gets or sets the level of this permission
	    /// </summary>
	    /// <value>The level.</value>
	    public virtual int Level { get; set; }

	    /// <summary>
	    /// Gets or sets the type of the entity.
	    /// </summary>
	    /// <value>The type of the entity.</value>
	    public virtual string EntityTypeName { get; set; }

	    /// <summary>
		/// Sets the type of the entity.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <remarks>
		/// This uses the weak assembly name to protect us from versioning issues
		/// </remarks>
		public virtual void SetEntityType(Type type)
		{
			EntityTypeName = type.FullName + ", " + type.Assembly.GetName().Name;
		}
	}
}