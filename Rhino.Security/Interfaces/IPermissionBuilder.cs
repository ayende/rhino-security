using Rhino.Security.Model;

namespace Rhino.Security.Interfaces
{
	/// <summary>
	/// Save the created permission
	/// </summary>
	public interface IPermissionBuilder
	{
		/// <summary>
		/// Save the created permission
		/// </summary>
		Permission Save();
        
        /// <summary>
		/// Builds a permission without saving
		/// </summary>
		Permission Build();
	}
}