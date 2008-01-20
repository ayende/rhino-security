namespace Rhino.Security
{
	using System.Text;

	/// <summary>
    /// Authorization Information relating to a specific 
    /// user/operation/entity.
    /// Allows to display the reasons for granting/denying permissions
    /// </summary>
    public class AuthorizationInformation
    {
		readonly StringBuilder builder = new StringBuilder();

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
		public override string ToString()
		{
			return builder.ToString();
		}


		/// <summary>
		/// Adds the specified formatted message that explains 
		/// why permission was allowed
		/// </summary>
		/// <param name="format">The format.</param>
		/// <param name="args">The args.</param>
    	public void AddAllow(string format, params object[] args)
    	{
    		builder.AppendFormat(format, args).AppendLine();
    	}


		/// <summary>
		/// Adds the specified formatted message that explains 
		/// why permission was denied
		/// </summary>
		/// <param name="format">The format.</param>
		/// <param name="args">The args.</param>
    	public void AddDeny(string format, params object[] args)
    	{
    		builder.AppendFormat(format, args).AppendLine();
    	}
    }
}