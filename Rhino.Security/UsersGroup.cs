namespace Rhino.Security
{
    using Castle.ActiveRecord;
    using Iesi.Collections.Generic;

    /// <summary>
    /// A named group for users, which we 
    /// can define operations on.
    /// </summary>
    [ActiveRecord]
    public class UsersGroup : NamedEntity<UsersGroup>
    {
        private UsersGroup parent;
        private ISet<UsersGroup> children = new HashedSet<UsersGroup>();
        private ISet<IUser> users = new HashedSet<IUser>();

        /// <summary>
        /// Gets or sets the users.
        /// </summary>
        /// <value>The users.</value>
        [HasAndBelongsToMany(
            Table = "UsersToUsersGroups",
            ColumnKey = "GroupId",
            ColumnRef = "UserId"
            )]
        public virtual ISet<IUser> Users
        {
            get { return users; }
            set { users = value; }
        }

        /// <summary>
        /// Gets or sets the parent of this group
        /// </summary>
        /// <value>The parent.</value>
        [BelongsTo]
        public virtual UsersGroup Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        /// <summary>
        /// Gets or sets the children of this group
        /// </summary>
        /// <value>The children.</value>
        [HasMany(Inverse = true)]
        public virtual ISet<UsersGroup> Children
        {
            get { return children; }
            set { children = value; }
        }
    }
}