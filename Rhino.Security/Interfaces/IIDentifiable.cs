using System;

namespace Rhino.Security.Interfaces
{
	/// <summary>
	/// Mark an entity with an id
	/// </summary>
	public interface IIDentifiable
	{
		/// <summary>
		/// Gets or sets the id.
		/// </summary>
		/// <value>The id.</value>
		Guid Id { get; set; }
	}
}