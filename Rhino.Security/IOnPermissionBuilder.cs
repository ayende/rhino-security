namespace Rhino.Security
{
    /// <summary>
    /// Define who this permission is on
    /// </summary>
    public interface IOnPermissionBuilder
    {
        /// <summary>
        /// Set the entity this permission is built for
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        ILevelPermissionBuilder On<TEntity>(TEntity entity) where TEntity : class;

        /// <summary>
        /// Set the entity group this permission is built for
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        ILevelPermissionBuilder On(EntitiesGroup entity);

        /// <summary>
        /// Set the entity group this permission is built for
        /// </summary>
        /// <param name="entitiesGroupName">Name of the entities group.</param>
        /// <returns></returns>
        ILevelPermissionBuilder On(string entitiesGroupName);

		/// <summary>
		/// Set this permission to be application to everything
		/// </summary>
		/// <returns></returns>
    	ILevelPermissionBuilder OnEverything();
    }
}