namespace Rhino.Security
{
    using System;
    using Commons;

    /// <summary>
    /// Allow to define permissions using a fluent interface
    /// </summary>
    public class PermissionsBuilderService : IPermissionsBuilderService
    {
        private readonly IRepository<Permission> permissionRepository;
        private readonly IAuthorizationEditingService authorizationEditingService;
        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionsBuilderService"/> class.
        /// </summary>
        /// <param name="permissionRepository">The permission repository.</param>
        /// <param name="authorizationEditingService">The authorization editing service.</param>
        public PermissionsBuilderService(IRepository<Permission> permissionRepository, IAuthorizationEditingService authorizationEditingService)
        {
            this.permissionRepository = permissionRepository;
            this.authorizationEditingService = authorizationEditingService;
        }

        /// <summary>
        /// Builds a permission
        /// </summary>
        public class FluentPermissionBuilder : IPermissionBuilder, IForPermissionBuilder, IOnPermissionBuilder,
                                         ILevelPermissionBuilder
        {
            private readonly Permission permission = new Permission();
            private readonly PermissionsBuilderService permissionBuilderService;

            /// <summary>
            /// Initializes a new instance of the <see cref="FluentPermissionBuilder"/> class.
            /// </summary>
            /// <param name="permissionBuilderService">The permission service.</param>
            /// <param name="allow">if set to <c>true</c> create an allow permission.</param>
            /// <param name="operation">The operation.</param>
            public FluentPermissionBuilder(PermissionsBuilderService permissionBuilderService, bool allow, Operation operation)
            {
                this.permissionBuilderService = permissionBuilderService;
                permission.Allow = allow;
                permission.Operation = operation;
            }

            /// <summary>
            /// Save the created permission
            /// </summary>
            public Permission Save()
            {
                permissionBuilderService.Save(permission);
                return permission;
            }

            /// <summary>
            /// Set the user that this permission is built for
            /// </summary>
            /// <param name="user">The user.</param>
            /// <returns></returns>
            public IOnPermissionBuilder For(IUser user)
            {
                permission.User = user;
                return this;
            }


            /// <summary>
            /// Set the users group that this permission is built for
            /// </summary>
            /// <param name="usersGroupName">Name of the users group.</param>
            /// <returns></returns>
            public IOnPermissionBuilder For(string usersGroupName)
            {
                UsersGroup usersGroup = permissionBuilderService
                    .authorizationEditingService
                    .GetUsersGroupByName(usersGroupName);

                Guard.Against<ArgumentException>(usersGroup == null, "There is not users group named: " + usersGroup);

                permission.UsersGroup = usersGroup;

                return this;
            }

            /// <summary>
            /// Set the entity this permission is built for
            /// </summary>
            /// <typeparam name="TEntity">The type of the entity.</typeparam>
            /// <param name="entity">The account.</param>
            /// <returns></returns>
            public ILevelPermissionBuilder On<TEntity>(TEntity entity) where TEntity : class
            {
                permission.SetEntityType(typeof (TEntity));
                permission.EntitySecurityKey = Security.ExtractKey(entity);
                return this;
            }


            /// <summary>
            /// Set the entity group this permission is built for
            /// </summary>
            /// <param name="entitiesGroupName">Name of the entities group.</param>
            /// <returns></returns>
            public ILevelPermissionBuilder On(string entitiesGroupName)
            {
                EntitiesGroup entitiesGroup = 
                    permissionBuilderService
                    .authorizationEditingService
                    .GetEntitiesGroupByName(entitiesGroupName);
                Guard.Against<ArgumentException>(entitiesGroup == null,
                                                 "There is no entities group named: " + entitiesGroupName);
                return On(entitiesGroup);
            }

            /// <summary>
            /// Set the entity group this permission is built for
            /// </summary>
            /// <param name="entitiesGroup">The entities group.</param>
            /// <returns></returns>
            public ILevelPermissionBuilder On(EntitiesGroup entitiesGroup)
            {
                permission.EntitiesGroup = entitiesGroup;
                return this;
            }


            /// <summary>
			/// Set this permission to be application to everything
			/// </summary>
			/// <returns></returns>
        	public ILevelPermissionBuilder OnEverything()
        	{
        		return this;
        	}

        	/// <summary>
            /// Define the level of this permission
            /// </summary>
            /// <param name="level">The level.</param>
            /// <returns></returns>
            public IPermissionBuilder Level(int level)
            {
                permission.Level = level;
                return this;
            }


            /// <summary>
            /// Define the default level;
            /// </summary>
            /// <returns></returns>
            public IPermissionBuilder DefaultLevel()
            {
                return Level(1);
            }
        }

        /// <summary>
        /// Saves the specified permission
        /// </summary>
        /// <param name="permission">The permission.</param>
        public void Save(Permission permission)
        {
            permissionRepository.Save(permission);
        }

        /// <summary>
        /// Allow permission for the specified operation.
        /// </summary>
        /// <param name="operationName">Name of the operation.</param>
        /// <returns></returns>
        public IForPermissionBuilder Allow(string operationName)
        {
            Operation operation = authorizationEditingService.GetOperationByName(operationName);
            Guard.Against<ArgumentException>(operation == null, "There is no operation named: " + operationName);
            return Allow(operation);
        }

        /// <summary>
        /// Deny permission for the specified operation 
        /// </summary>
        /// <param name="operationName">Name of the operation.</param>
        /// <returns></returns>
        public IForPermissionBuilder Deny(string operationName)
        {
            Operation operation = authorizationEditingService.GetOperationByName(operationName);
            Guard.Against<ArgumentException>(operation == null, "There is no operation named: " + operationName);
            return Deny(operation);
        }


        /// <summary>
        /// Allow permission for the specified operation.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <returns></returns>
        public IForPermissionBuilder Allow(Operation operation)
        {
            return new FluentPermissionBuilder(this, true, operation);
        }

        /// <summary>
        /// Deny permission for the specified operation
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <returns></returns>
        public IForPermissionBuilder Deny(Operation operation)
        {
            return new FluentPermissionBuilder(this, false, operation);
        }
    }
}