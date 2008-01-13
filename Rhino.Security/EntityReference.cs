namespace Rhino.Security
{
    using System;
    using Castle.ActiveRecord;

    /// <summary>
    /// A reference to an entity in the domain
    /// </summary>
    [ActiveRecord]
    public class EntityReference : EqualityAndHashCodeProvider<EntityReference>
    {
        private EntityType type;
        private Guid entitySecurityKey;

        /// <summary>
        /// Gets or sets the entity security key.
        /// </summary>
        /// <value>The entity security key.</value>
        [Property(NotNull = true,Unique = true)]
        public virtual Guid EntitySecurityKey
        {
            get { return entitySecurityKey; }
            set { entitySecurityKey = value; }
        }

        /// <summary>
        /// Gets or sets the type of the entity this instance
        /// reference
        /// </summary>
        /// <value>The type.</value>
        [BelongsTo(NotNull = true)]
        public virtual EntityType Type
        {
            get { return type; }
            set { type = value; }
        }
    }
}