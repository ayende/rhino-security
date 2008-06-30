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
		private Operation operation;
		private Guid? entitySecurityKey;
		private EntitiesGroup entitiesGroup;
		private bool allow;
		private IUser user;
		private UsersGroup usersGroup;
		private int level;
		private string entityTypeName;

		/// <summary>
		/// Gets or sets the operation this permission applies to
		/// </summary>
		/// <value>The operation.</value>
		public virtual Operation Operation
		{
			get { return operation; }
			set { operation = value; }
		}

		/// <summary>
		/// Gets or sets the entity security key this permission belongs to
		/// </summary>
		/// <value>The entity security key.</value>
		public virtual Guid? EntitySecurityKey
		{
			get { return entitySecurityKey; }
			set { entitySecurityKey = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Permission"/> is allowing 
		/// or denying the operation.
		/// </summary>
		/// <value><c>true</c> if allow; otherwise, <c>false</c>.</value>
		public virtual bool Allow
		{
			get { return allow; }
			set { allow = value; }
		}

		/// <summary>
		/// Gets or sets the user this permission belongs to.
		/// </summary>
		/// <value>The user.</value>
		public virtual IUser User
		{
			get { return user; }
			set { user = value; }
		}

		/// <summary>
		/// Gets or sets the users group this permission belongs to
		/// </summary>
		/// <value>The users group.</value>
		public virtual UsersGroup UsersGroup
		{
			get { return usersGroup; }
			set { usersGroup = value; }
		}


		/// <summary>
		/// Gets or sets the entities group this permission belongs to
		/// </summary>
		/// <value>The entities group.</value>
		public virtual EntitiesGroup EntitiesGroup
		{
			get { return entitiesGroup; }
			set { entitiesGroup = value; }
		}

		/// <summary>
		/// Gets or sets the level of this permission
		/// </summary>
		/// <value>The level.</value>
		public virtual int Level
		{
			get { return level; }
			set { level = value; }
		}

		/// <summary>
		/// Gets or sets the type of the entity.
		/// </summary>
		/// <value>The type of the entity.</value>
		public virtual string EntityTypeName
		{
			get { return entityTypeName; }
			set { entityTypeName = value; }
		}

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