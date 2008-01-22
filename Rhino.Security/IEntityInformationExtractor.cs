namespace Rhino.Security
{
    using System;

    /// <summary>
    /// An extension point that allow user of rhino security
    /// to provide the information about an entity without
    /// having to modify the domain model or having to know anything about it
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public interface IEntityInformationExtractor<TEntity>
    {
        /// <summary>
        /// Gets the security key for the specified entity
        /// </summary>
        /// <param name="entity">The entity.</param>
        Guid GetSecurityKeyFor(TEntity entity);

        /// <summary>
        /// Gets the description of this security key, used when rhino security needs
        /// to generate a message that involves this security key
        /// </summary>
        /// <param name="securityKey">The security key.</param>
        /// <returns></returns>
        string GetDescription(Guid securityKey);

        /// <summary>
        /// Gets the name of the security key property.
        /// </summary>
        /// <value>The name of the security key property.</value>
        string SecurityKeyPropertyName { get; }
    }
}