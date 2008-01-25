namespace Rhino.Security
{
    /// <summary>
    /// A flag that determains how the security tables should be treated.
    /// In a separate schema or using a naming convention.
    /// The default is to put them in a separate schema.
    /// </summary>
    public enum SecurityTableStructure
    {
        /// <summary>
        /// Use a "security" schema
        /// </summary>
        Schema,
        /// <summary>
        /// Use a "security_" prefix
        /// </summary>
        Prefix
    }
}