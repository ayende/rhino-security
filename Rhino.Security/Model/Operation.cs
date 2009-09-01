using System.Collections.Generic;
using NHibernate.Validator.Constraints;

namespace Rhino.Security.Model
{
	/// <summary>
	/// Represent an operation in the system
	/// </summary>
	public class Operation : NamedEntity<Operation>
	{
	    ///<summary>
	    /// Create instance of <c>Operation</c>
	    ///</summary>
	    public Operation()
	    {
	        Children = new HashSet<Operation>();
	    }

	    /// <summary>
	    /// Gets or sets the comment for this operation
	    /// </summary>
	    /// <value>The comment.</value>
	    [Length(255), NotEmpty]
	    public virtual string Comment { get; set; }

	    /// <summary>
	    /// Gets or sets the parent of this operation
	    /// </summary>
	    /// <value>The parent.</value>
	    public virtual Operation Parent { get; set; }

	    /// <summary>
	    /// Gets or sets the children of this operation
	    /// </summary>
	    /// <value>The children.</value>
	    public virtual ICollection<Operation> Children { get; set; }
	}
}