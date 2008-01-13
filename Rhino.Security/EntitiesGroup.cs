namespace Rhino.Security
{
    using System;
    using Castle.ActiveRecord;
    using Commons;
    using Iesi.Collections.Generic;

    /// <summary>
    /// A grouping of entities, with some business meaning.
    /// </summary>
    [ActiveRecord]
    public class EntitiesGroup : NamedEntity<EntitiesGroup>
    {
        private ISet<EntityReference> entitiesKeys = new HashedSet<EntityReference>();

        /// <summary>
        /// Gets or sets the entities security keys beloging to this entities group
        /// </summary>
        /// <value>The entities security keys.</value>
        [HasAndBelongsToMany(
            Table = "EntityReferencesToEntitiesGroups",
            ColumnKey = "GroupId",
            ColumnRef = "EntityReferenceId"
            )]
        public virtual ISet<EntityReference> Entities
        {
            get { return entitiesKeys; }
            set { entitiesKeys = value; }
        }
    }
}