using Iesi.Collections.Generic;

namespace Rhino.Security.Model
{
	/// <summary>
	/// A grouping of entities, with some business meaning.
	/// </summary>
	public class EntitiesGroup : NamedEntity<EntitiesGroup>
	{
		private ISet<EntityReference> entities = new HashedSet<EntityReference>();

		/// <summary>
		/// Gets or sets the entities security keys beloging to this entities group
		/// </summary>
		/// <value>The entities security keys.</value>
		public virtual ISet<EntityReference> Entities
		{
			get { return entities; }
			set { entities = value; }
		}
	}
}