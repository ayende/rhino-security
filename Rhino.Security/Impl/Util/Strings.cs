using System.Collections.Generic;
using System.Text;
using Rhino.Security.Model;
using Rhino.Security.Properties;

namespace Rhino.Security.Impl.Util
{
	/// <summary>
	/// String utility methods
	/// </summary>
	public static class Strings
	{
		/// <summary>
		/// Gets the name of the parent operation.
		/// </summary>
		/// <param name="operationName">Name of the operation.</param>
		/// <remarks>
		/// Assumes that there is a '/' in the string
		/// </remarks>
		public static string GetParentOperationName(string operationName)
		{
			int lastIndex = operationName.LastIndexOf('/');
			return operationName.Substring(0, lastIndex);
		}

		/// <summary>
		/// Gets the names of all the parent operations (including the current one)
		/// </summary>
		/// <param name="operationName">Name of the operation.</param>
		/// <remarks>
		/// Assumes that there is a '/' in the string
		/// </remarks>
		public static string[] GetHierarchicalOperationNames(string operationName)
		{
			List<string> names = new List<string>();
			do
			{
				names.Add(operationName);
				operationName = GetParentOperationName(operationName);
			} while (operationName != "");
			return names.ToArray();
		}

		/// <summary>
		/// Joins the names of all the specified entities.
		/// </summary>
		/// <typeparam name="TNamedEntity">The type of the named entity.</typeparam>
		/// <param name="entities">The entities.</param>
		/// <param name="separator">The separator.</param>
		/// <returns></returns>
		public static string Join<TNamedEntity>(NamedEntity<TNamedEntity>[] entities, string separator) where TNamedEntity : NamedEntity<TNamedEntity>
		{
			StringBuilder sb = new StringBuilder();
			foreach (NamedEntity<TNamedEntity> entity in entities)
			{
				sb.Append(entity.Name).Append(separator);
			}
			if (sb.Length == 0)
				return Resources.NotAssociatedWithAnyGroup;
			sb.Remove(sb.Length - separator.Length, separator.Length);
			return sb.ToString();
		}

		/// <summary>
		/// Joins the names of all the specified entities.
		/// </summary>
		/// <typeparam name="TNamedEntity">The type of the named entity.</typeparam>
		/// <param name="entities">The entities.</param>
		/// <returns></returns>
		public static string Join<TNamedEntity>(NamedEntity<TNamedEntity>[] entities) where TNamedEntity : NamedEntity<TNamedEntity>
		{
			return Join(entities, ", ");
		}
	}
}