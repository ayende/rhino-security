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
        private ISet<UsersGroup> directChildren = new HashedSet<UsersGroup>();
        private ISet<UsersGroup> allChildren = new HashedSet<UsersGroup>();
        private ISet<UsersGroup> allParent = new HashedSet<UsersGroup>();
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
        /// Gets or sets the direct children of this group (nested one level)
        /// </summary>
        /// <value>The directChildren.</value>
        [HasMany(Inverse = true)]
        public virtual ISet<UsersGroup> DirectChildren
        {
            get { return directChildren; }
            set { directChildren = value; }
        }


        /// <summary>
        /// Gets or sets all children of this users group, at all nesting levels
        /// </summary>
        /// <value>All children.</value>
        [HasAndBelongsToMany(
            Table = "UsersGroupsHierarchy",
            ColumnKey = "ParentGroup",
            ColumnRef = "ChildGroup",
            Inverse = true
            )]
        public virtual ISet<UsersGroup> AllChildren
        {
            get { return allChildren; }
            set { allChildren = value; }
        }


        /// <summary>
        /// Gets or sets all parent of this users group, at all nesting levels
        /// </summary>
        /// <value>All children.</value>
        [HasAndBelongsToMany(
            Table = "UsersGroupsHierarchy",
            ColumnRef = "ParentGroup",
           ColumnKey = "ChildGroup"
            )]
        public virtual ISet<UsersGroup> AllParents
        {
            get { return allParent; }
            set { allParent = value; }
        }
    }
}