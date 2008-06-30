using Castle.Components.Validator;
using Iesi.Collections.Generic;
using Rhino.Security.Model;

namespace Rhino.Security.Model
{
	/// <summary>
	/// Represent an operation in the system
	/// </summary>
	public class Operation : NamedEntity<Operation>
	{
		private string comment;
		private Operation parent;
		private ISet<Operation> children = new HashedSet<Operation>();

		/// <summary>
		/// Gets or sets the comment for this operation
		/// </summary>
		/// <value>The comment.</value>
		[ValidateLength(0, 255)]
		public virtual string Comment
		{
			get { return comment; }
			set { comment = value; }
		}

		/// <summary>
		/// Gets or sets the parent of this operation
		/// </summary>
		/// <value>The parent.</value>
		public virtual Operation Parent
		{
			get { return parent; }
			set { parent = value; }
		}

		/// <summary>
		/// Gets or sets the children of this operation
		/// </summary>
		/// <value>The children.</value>
		public virtual ISet<Operation> Children
		{
			get { return children; }
			set { children = value; }
		}
	}
}