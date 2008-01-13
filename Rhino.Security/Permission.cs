namespace Rhino.Security
{
    using System;
    using Castle.ActiveRecord;

    /// <summary>
    /// Represent a permission on the system, allow (or denying) 
    /// [operation] for [someone] on [something]
    /// </summary>
    [ActiveRecord]
    public class Permission : EqualityAndHashCodeProvider<Permission>
    {
        private Operation operation;
        private Guid? entitySecurityKey;
        private EntitiesGroup entitiesGroup;
        private bool allow;
        private IUser user;
        private UsersGroup usersGroup;
        private int level;

        /// <summary>
        /// Gets or sets the operation this permission applies to
        /// </summary>
        /// <value>The operation.</value>
        [BelongsTo(NotNull = true)]
        public virtual Operation Operation
        {
            get { return operation; }
            set { operation = value; }
        }

        /// <summary>
        /// Gets or sets the entity security key this permission belongs to
        /// </summary>
        /// <value>The entity security key.</value>
        [Property]
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
        [Property(NotNull = true)]
        public virtual bool Allow
        {
            get { return allow; }
            set { allow = value; }
        }

        /// <summary>
        /// Gets or sets the user this permission belongs to.
        /// </summary>
        /// <value>The user.</value>
        [BelongsTo("`User`")]
        public virtual IUser User
        {
            get { return user; }
            set { user = value; }
        }

        /// <summary>
        /// Gets or sets the users group this permission belongs to
        /// </summary>
        /// <value>The users group.</value>
        [BelongsTo]
        public virtual UsersGroup UsersGroup
        {
            get { return usersGroup; }
            set { usersGroup = value; }
        }


        /// <summary>
        /// Gets or sets the entities group this permission belongs to
        /// </summary>
        /// <value>The entities group.</value>
        [BelongsTo]
        public virtual EntitiesGroup EntitiesGroup
        {
            get { return entitiesGroup; }
            set { entitiesGroup = value; }
        }

        /// <summary>
        /// Gets or sets the level of this permission
        /// </summary>
        /// <value>The level.</value>
        [Property(NotNull = true)]
        public virtual int Level
        {
            get { return level; }
            set { level = value; }
        }
    }
}