namespace Rhino.Security
{
    /// <summary>
    /// Allow to retrieve and remove permissions
    /// on users, user groups, entities groups and entities.
    /// </summary>
    public interface IPermissionsService
    {
        /// <summary>
        /// Gets the permissions for the specified user
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        Permission[] GetPermissionsFor(IUser user);

        /// <summary>
        /// Gets the permissions for the specified user and entity
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="user">The user.</param>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        Permission[] GetPermissionsFor<TEntity>(IUser user, TEntity entity) where TEntity : class;

        /// <summary>
        /// Gets the permissions for the specified etntity
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="operationName">Name of the operation.</param>
        /// <returns></returns>
        Permission[] GetPermissionsFor(IUser user, string operationName) ;

        /// <summary>
        /// Gets the permissions for the specified etntity
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="user">The user.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="operationName">Name of the operation.</param>
        /// <returns></returns>
        Permission[] GetPermissionsFor<TEntity>(IUser user, TEntity entity, string operationName) where TEntity : class;

        /// <summary>
        /// Gets the permissions for the specified etntity
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        Permission[] GetPermissionsFor<TEntity>(TEntity entity) where TEntity : class;

        /// <summary>
        /// Removes the specified permission.
        /// </summary>
        /// <param name="permission">The permission.</param>
        void RemovePermission(Permission permission);
    }
}