namespace Rhino.Security
{
    using System;
    using System.Collections.Generic;
    using Castle.Components.Validator;
    using Commons;
    using NHibernate.Expressions;
    using NHibernate.SqlCommand;

    /// <summary>
    /// Allows to edit the security information of the 
    /// system
    /// </summary>
    public class AuthorizationEditingService : IAuthorizationEditingService
    {
        private readonly IRepository<EntitiesGroup> entitiesGroupRepository;
        private readonly IRepository<EntityReference> entityReferenceRepository;
        private readonly IRepository<EntityType> entityTypesRepository;
        private readonly IRepository<Operation> operationsRepository;
        private readonly IRepository<UsersGroup> usersGroupRepository;
        private readonly ValidatorRunner validator;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationEditingService"/> class.
        /// </summary>
        /// <param name="usersGroupRepository">The users group repository.</param>
        /// <param name="entitiesGroupRepository">The entities group repository.</param>
        /// <param name="validator">The validator.</param>
        /// <param name="entityReferenceRepository">The entity reference repository.</param>
        /// <param name="entityTypesRepository">The entity types repository.</param>
        /// <param name="operationsRepository">The operations repository.</param>
        public AuthorizationEditingService(IRepository<UsersGroup> usersGroupRepository,
                                           IRepository<EntitiesGroup> entitiesGroupRepository, ValidatorRunner validator,
                                           IRepository<EntityReference> entityReferenceRepository,
                                           IRepository<EntityType> entityTypesRepository,
                                           IRepository<Operation> operationsRepository)
        {
            this.usersGroupRepository = usersGroupRepository;
            this.operationsRepository = operationsRepository;
            this.entityTypesRepository = entityTypesRepository;
            this.entityReferenceRepository = entityReferenceRepository;
            this.entitiesGroupRepository = entitiesGroupRepository;
            this.validator = validator;
        }

        #region IAuthorizationEditingService Members

        /// <summary>
        /// Creates a new users group.
        /// </summary>
        /// <param name="name">The name of the new group.</param>
        public virtual UsersGroup CreateUsersGroup(string name)
        {
            UsersGroup ug = new UsersGroup();
            ug.Name = name;
            if (validator.IsValid(ug) == false)
            {
                ErrorSummary summary = validator.GetErrorSummary(ug);
                throw new ValidationException(summary);
            }
            usersGroupRepository.Save(ug);
            return ug;
        }


        /// <summary>
        /// Creates the users group as a child of <paramref name="parentGroupName"/>.
        /// </summary>
        /// <param name="parentGroupName">Name of the parent group.</param>
        /// <param name="usersGroupName">Name of the users group.</param>
        /// <returns></returns>
        public UsersGroup CreateChildUserGroupOf(string parentGroupName, string usersGroupName)
        {
            UsersGroup parent = GetUsersGroupByName(parentGroupName);
            Guard.Against<ArgumentException>(parent == null,
                                             "Parent users group '" + parentGroupName + "' does not exists");

            UsersGroup group = CreateUsersGroup(usersGroupName);
            group.Parent = parent;
            group.AllParents.AddAll(parent.AllParents);
            group.AllParents.Add(parent);
            parent.Directchildren.Add(group);
            parent.AllChildren.Add(group);
            return group;
        }


        /// <summary>
        /// Gets the ancestry association of a user with the named users group.
        /// This allows to track how a user is associated to a group through 
        /// their ancestry.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="usersGroupName">Name of the users group.</param>
        /// <returns></returns>
        public UsersGroup[] GetAncestryAssociation(IUser user, string usersGroupName)
        {
            UsersGroup desiredGroup = GetUsersGroupByName(usersGroupName);
            ICollection<UsersGroup> directGroups =
                usersGroupRepository.FindAll(GetDirectUserGroupsCriteria(user));
            if (directGroups.Contains(desiredGroup))
            {
                return new UsersGroup[] { desiredGroup };
            }
            // as a nice benefit, this does an eager load of all the groups in the hierarchy
            // in an efficent way, so we don't have SELECT N + 1 here, nor do we need
            // to load the Users collection (which may be very large) to check if we are associated
            UsersGroup[] associatedGroups = GetAssociatedUsersGroupFor(user);
            if (Array.IndexOf(associatedGroups, desiredGroup) == -1)
            {
                return new UsersGroup[0];
            }
            // now we need to find out the path to it
            List<UsersGroup> shortest = new List<UsersGroup>();
            foreach (UsersGroup usersGroup in associatedGroups)
            {
                List<UsersGroup> path = new List<UsersGroup>();
                UsersGroup current = usersGroup;
                while (current.Parent != null && current != desiredGroup)
                {
                    path.Add(current);
                    current = current.Parent;
                }
                if (current != null)
                    path.Add(current);
                // Valid paths are those that are contains the desired group
                // and start in one of the groups that are directly associated
                // with the user
                if (path.Contains(desiredGroup) && directGroups.Contains(path[0]))
                {
                    shortest = Min(shortest, path);
                }
            }
            return shortest.ToArray();
        }

        /// <summary>
        /// Creates a new entities group.
        /// </summary>
        /// <param name="name">The name of the new group.</param>
        public virtual EntitiesGroup CreateEntitiesGroup(string name)
        {
            EntitiesGroup eg = new EntitiesGroup();
            eg.Name = name;
            if (validator.IsValid(eg) == false)
            {
                ErrorSummary summary = validator.GetErrorSummary(eg);
                throw new ValidationException(summary);
            }
            entitiesGroupRepository.Save(eg);
            return eg;
        }

        /// <summary>
        /// Gets the associated users group for the specified user.
        /// </summary>
        /// <param name="user">The user.</param>
        public virtual UsersGroup[] GetAssociatedUsersGroupFor(IUser user)
        {
            DetachedCriteria directGroupsCriteria = GetDirectUserGroupsCriteria(user)
                .SetProjection(Projections.Id());

            DetachedCriteria allGroupsCriteria = DetachedCriteria.For<UsersGroup>()
                .CreateAlias("Users", "user", JoinType.LeftOuterJoin)
                .CreateAlias("AllChildren", "child", JoinType.LeftOuterJoin)
                .Add(
                Subqueries.PropertyIn("child.id", directGroupsCriteria) ||
                Expression.Eq("user.id", user.SecurityInfo.Identifier));

            ICollection<UsersGroup> usersGroups =
                usersGroupRepository.FindAll(allGroupsCriteria, Order.Asc("Name"));
            return Collection.ToArray<UsersGroup>(usersGroups);
        }


        /// <summary>
        /// Gets the users group by its name
        /// </summary>
        /// <param name="groupName">Name of the group.</param>
        public virtual UsersGroup GetUsersGroupByName(string groupName)
        {
            return usersGroupRepository.FindOne(Expression.Eq("Name", groupName));
        }

        /// <summary>
        /// Gets the entities group by its groupName
        /// </summary>
        /// <param name="groupName">The name of the group.</param>
        public virtual EntitiesGroup GetEntitiesGroupByName(string groupName)
        {
            return entitiesGroupRepository.FindOne(Expression.Eq("Name", groupName));
        }


        /// <summary>
        /// Gets the groups the specified entity is associated with
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public virtual EntitiesGroup[] GetAssociatedEntitiesGroupsFor<TEntity>(TEntity entity) where TEntity : class
        {
            Guid key = Security.ExtractKey(entity);
            DetachedCriteria criteria = DetachedCriteria.For<EntitiesGroup>()
                .CreateAlias("Entities", "e")
                .Add(Expression.Eq("e.EntitySecurityKey", key));
            ICollection<EntitiesGroup> entitiesGroups = entitiesGroupRepository.FindAll(criteria);
            return Collection.ToArray<EntitiesGroup>(entitiesGroups);
        }

        /// <summary>
        /// Associates the entity with a group with the specified name
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="groupName">Name of the group.</param>
        public virtual void AssociateEntityWith<TEntity>(TEntity entity, string groupName) where TEntity : class
        {
            EntitiesGroup entitiesGroup = GetEntitiesGroupByName(groupName);
            Guard.Against<ArgumentException>(entitiesGroup == null, "There is no entities group named: " + groupName);

            Guid key = Security.ExtractKey(entity);

            EntityReference reference = GetOrCreateEntityReference<TEntity>(key);
            entitiesGroup.Entities.Add(reference);
        }


        /// <summary>
        /// Associates the user with a group with the specified name
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="groupName">Name of the group.</param>
        public virtual void AssociateUserWith(IUser user, string groupName)
        {
            UsersGroup group = GetUsersGroupByName(groupName);
            Guard.Against(group == null, "There is no users group named: " + groupName);

            group.Users.Add(user);
        }


        /// <summary>
        /// Creates the operation with the given name
        /// </summary>
        /// <param name="operationName">Name of the operation.</param>
        /// <returns></returns>
        public virtual Operation CreateOperation(string operationName)
        {
            Guard.Against<ArgumentException>(string.IsNullOrEmpty(operationName), "operationName must have a value");
            Guard.Against<ArgumentException>(operationName[0] != '/', "Operation names must start with '/'");

            Operation op = new Operation();
            op.Name = operationName;

            if (validator.IsValid(op) == false)
                throw new ValidationException(validator.GetErrorSummary(op));

            string parentOperationName = Strings.GetParentOperationName(operationName);
            if (parentOperationName != string.Empty) //we haven't got to the root
            {
                Operation parentOperation = GetOperationByName(parentOperationName);
                if (parentOperation == null)
                    parentOperation = CreateOperation(parentOperationName);

                op.Parent = parentOperation;
                parentOperation.Children.Add(op);
            }

            operationsRepository.Save(op);
            return op;
        }

        /// <summary>
        /// Gets the operation by the specified name
        /// </summary>
        /// <param name="operationName">Name of the operation.</param>
        /// <returns></returns>
        public virtual Operation GetOperationByName(string operationName)
        {
            return operationsRepository.FindOne(Expression.Eq("Name", operationName));
        }

        /// <summary>
        /// Removes the user from the specified group
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="usersGroupName">Name of the users group.</param>
        public void DetachUserFromGroup(IUser user, string usersGroupName)
        {
            UsersGroup group = GetUsersGroupByName(usersGroupName);
            Guard.Against(group == null, "There is no users group named: " + usersGroupName);

            group.Users.Remove(user);
        }

        /// <summary>
        /// Removes the entities from the specified group
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="entitiesGroupName">Name of the entities group.</param>
        public void DetachEntityFromGroup<TEntity>(TEntity entity, string entitiesGroupName) where TEntity : class
        {
            EntitiesGroup entitiesGroup = GetEntitiesGroupByName(entitiesGroupName);
            Guard.Against<ArgumentException>(entitiesGroup == null,
                                             "There is no entities group named: " + entitiesGroupName);
            Guid key = Security.ExtractKey(entity);

            EntityReference reference = GetOrCreateEntityReference<TEntity>(key);
            entitiesGroup.Entities.Remove(reference);
        }

        #endregion

        private static List<UsersGroup> Min(List<UsersGroup> first, List<UsersGroup> second)
        {
            if (first.Count == 0)
                return second;
            if (first.Count <= second.Count)
                return first;
            return second;
        }

        private DetachedCriteria GetDirectUserGroupsCriteria(IUser user)
        {
            return DetachedCriteria.For<UsersGroup>()
                .CreateAlias("Users", "user")
                .Add(Expression.Eq("user.id", user.SecurityInfo.Identifier));
        }

        private EntityReference GetOrCreateEntityReference<TEntity>(Guid key)
        {
            EntityReference reference = entityReferenceRepository.FindOne(Expression.Eq("EntitySecurityKey", key));
            if (reference == null)
            {
                reference = new EntityReference();
                reference.EntitySecurityKey = key;
                reference.Type = GetOrCreateEntityType<TEntity>();
                entityReferenceRepository.Save(reference);
            }
            return reference;
        }

        private EntityType GetOrCreateEntityType<TEntity>()
        {
            EntityType entityType = entityTypesRepository.FindOne(
                Expression.Eq("Name", typeof(TEntity).FullName)
                );
            if (entityType == null)
            {
                entityType = new EntityType();
                entityType.Name = typeof(TEntity).FullName;
                entityTypesRepository.Save(entityType);
            }
            return entityType;
        }
    }
}