namespace Rhino.Security
{
    using System.Collections.Generic;

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
    }
}