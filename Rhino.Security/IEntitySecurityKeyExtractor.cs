namespace Rhino.Security
{
    using System;

    /// <summary>
    /// An extension point that allow user of rhino security
    /// to provide the security key of an entity without
    /// having to modify their domain model
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public interface IEntitySecurityKeyExtractor<TEntity>
    {
        /// <summary>
        /// Gets the security key for the specified entity
        /// </summary>
        /// <param name="entity">The entity.</param>
        Guid GetSecurityKeyFor(TEntity entity);
    }
}