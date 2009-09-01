using System.Collections.Generic;

namespace Rhino.Security.Model
{
	/// <summary>
	/// A grouping of entities, with some business meaning.
	/// </summary>
	public class EntitiesGroup : NamedEntity<EntitiesGroup>
	{
	    ///<summary>
        /// Create a new instance of <c>EntitiesGroup</c>
	    ///</summary>
	    public EntitiesGroup()
	    {
	        Entities = new HashSet<EntityReference>();
	    }

	    /// <summary>
	    /// Gets or sets the entities security keys beloging to this entities group
	    /// </summary>
	    /// <value>The entities security keys.</value>
	    public virtual ICollection<EntityReference> Entities { get; set; }
	}
}