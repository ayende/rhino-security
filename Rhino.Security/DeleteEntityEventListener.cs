namespace Rhino.Security
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NHibernate.Criterion;
    using NHibernate.Event;
    using Rhino.Security.Model;

    /// <summary>
    /// Litenens for when a secured entity is deleted from the system and deletes 
    /// associated security data.
    /// </summary>
    [Serializable]
    class DeleteEntityEventListener : IPreDeleteEventListener
    {
        #region IPreDeleteEventListener Members

        /// <summary>
        /// Handles PreDelete event to delete an entity's associated security data.
        /// </summary>
        /// <param name="deleteEvent">Event object containing the delete operation information.</param>
        /// <returns>False, indicating the delete operation should not be vetoed.</returns>
        public bool OnPreDelete(PreDeleteEvent deleteEvent)
        {
            var securityKey = Security.ExtractKey(deleteEvent.Entity);

            if (Guid.Empty.Equals(securityKey))
                return false;

            EntityReference entityReference = deleteEvent.Session.CreateCriteria<EntityReference>()
                    .Add(Restrictions.Eq("EntitySecurityKey", securityKey))
                    .SetCacheable(true)
                    .UniqueResult<EntityReference>();

            if (entityReference == null)
                return false;

            var childSession = deleteEvent.Session.GetSession(NHibernate.EntityMode.Poco);

            //Also remove EntityReferencesToEntitiesGroups and Permissions that reference this entity

            //Get list of EntitiesGroups that have the entity as a member
            IEnumerable<EntitiesGroup> entitiesGroups = childSession.CreateCriteria<EntitiesGroup>()
                .CreateCriteria("Entities")
                .Add(Restrictions.Eq("EntitySecurityKey", securityKey))
                .SetCacheable(true)
                .List<EntitiesGroup>();

            foreach (EntitiesGroup group in entitiesGroups)
            {
                foreach (var entityRef in group.Entities.Where(x => x.EntitySecurityKey == entityReference.EntitySecurityKey).ToList())
                    group.Entities.Remove(entityRef);
            }

            ////Get list of Permissions that references the entity
            IEnumerable<Permission> permissions = childSession.CreateCriteria<Permission>()
                .Add(Restrictions.Eq("EntitySecurityKey", securityKey))
                .SetCacheable(true)
                .List<Permission>();

            foreach (Permission permission in permissions)
            {
                childSession.Delete(permission);
            }

            childSession.Flush();

            childSession.CreateQuery("delete from Rhino.Security.Model.EntityReference er where er.Id=:id")
                .SetParameter("id", entityReference.Id)
                .ExecuteUpdate();

            return false;
        }

        #endregion
    }
}
