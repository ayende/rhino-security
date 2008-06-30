namespace Rhino.Security.Interfaces
{
	/// <summary>
	/// Define what is the level of this permission
	/// </summary>
	public interface ILevelPermissionBuilder
	{
		/// <summary>
		/// Define the level of this permission
		/// </summary>
		/// <param name="level">The level.</param>
		/// <returns></returns>
		IPermissionBuilder Level(int level);

		/// <summary>
		/// Define the default level;
		/// </summary>
		/// <returns></returns>
		IPermissionBuilder DefaultLevel();
	}
}