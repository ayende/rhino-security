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
	        AllParents = new HashSet<EntitiesGroup>();
            AllChildren = new HashSet<EntitiesGroup>();
            DirectChildren = new HashSet<EntitiesGroup>();
	    }

	    /// <summary>
	    /// Gets or sets the entities security keys beloging to this entities group
	    /// </summary>
	    /// <value>The entities security keys.</value>
	    public virtual ICollection<EntityReference> Entities { get; set; }


        /// <summary>
        /// Gets or sets the parent of this group
        /// </summary>
        /// <value>The parent.</value>
        public virtual EntitiesGroup Parent { get; set; }


        /// <summary>
        /// Gets or sets the direct children of this group (nested one level)
        /// </summary>
        /// <value>The directChildren.</value>
        public virtual ICollection<EntitiesGroup> DirectChildren { get; set; }


        /// <summary>
        /// Gets or sets all children of this users group, at all nesting levels
        /// </summary>
        /// <value>All children.</value>
        public virtual ICollection<EntitiesGroup> AllChildren { get; set; }
        
        
        /// <summary>
        /// Gets or sets all parent of this users group, at all nesting levels
        /// </summary>
        /// <value>All children.</value>
        public virtual ICollection<EntitiesGroup> AllParents { get; set; }

	}
}