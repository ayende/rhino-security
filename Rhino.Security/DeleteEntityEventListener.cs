namespace Rhino.Security
{
	using System;
	using NHibernate.Event;
	using Microsoft.Practices.ServiceLocation;
	using Rhino.Security.Interfaces;
	using System.Collections.Generic;
	using Rhino.Security.Model;
	using Rhino.Security.Services;
	using NHibernate.Criterion;

	/// <summary>
	/// Litenens for when a secured entity is deleted from the system and deletes 
	/// associated security data.
	/// </summary>
	[Serializable]
	public class DeleteEntityEventListener : IPreDeleteEventListener
	{
		#region IPreDeleteEventListener Members

		/// <summary>
		/// Handles PreDelete event to delete an entity's associated security data.
		/// </summary>
		/// <param name="deleteEvent">Event object containing the delete operation information.</param>
		/// <returns>False, indicating the delete operation should not be vetoed.</returns>
		public bool OnPreDelete(PreDeleteEvent deleteEvent) {
			var securityKey = Security.ExtractKey(deleteEvent.Entity);

			if (!Guid.Empty.Equals(securityKey)) {
				EntityReference reference = deleteEvent.Session.CreateCriteria<EntityReference>()
					 .Add(Restrictions.Eq("EntitySecurityKey", securityKey))
					 .SetCacheable(true)
					 .UniqueResult<EntityReference>();

				if (reference != null) {
					var childSession = deleteEvent.Session.GetSession(NHibernate.EntityMode.Poco);
					childSession.Delete(reference);
					childSession.Flush();
				}
			}

			return false;
		}

		#endregion
	}
}
