namespace Rhino.Security
{
    /// <summary>
    /// Provide a way to get the security information from a user instance.
    /// Used to separate the domain model's user from the requirements of the 
    /// security systems
    /// </summary>
    public interface IUser
    {
        /// <summary>
        /// Gets or sets the security info for this user
        /// </summary>
        /// <value>The security info.</value>
        SecurityInfo SecurityInfo { get; }
    }
}