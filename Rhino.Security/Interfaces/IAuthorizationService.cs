using NHibernate;
using NHibernate.Criterion;
using Rhino.Security.Model;
using System.Linq;

namespace Rhino.Security.Interfaces
{
	///<summary>
	/// Implementors of this interface are able to answer
	/// on authorization questions as well as enhance Criteria
	/// queries
	///</summary>
	public interface IAuthorizationService
	{
        /// <summary>
        /// Adds the permissions to the linq query.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="operation">The operation.</param>
        /// <param name="query">The query.</param>
        IQueryable<TEntity> AddPermissionsToQuery<TEntity>(IUser user, string operation, IQueryable<TEntity> query) where TEntity : class;

        ///<summary>
        /// Adds the permissions to the linq query for the given usersgroup
        ///</summary>
        ///<param name="usersgroup">The usersgroup. Only permissions directly related to this usergroup 
        /// are taken into account</param>
        ///<param name="operation">The operation</param>
        /// <param name="query">The query.</param>
        IQueryable<TEntity> AddPermissionsToQuery<TEntity>(UsersGroup usersgroup, string operation, IQueryable<TEntity> query) where TEntity : class;

        /// <summary>
		/// Adds the permissions to the criteria query.
		/// </summary>
		/// <param name="user">The user.</param>
		/// <param name="criteria">The criteria.</param>
		/// <param name="operation">The operation.</param>
		void AddPermissionsToQuery(IUser user, string operation, ICriteria criteria);

        ///<summary>
        /// Adds the permissions to the criteria query for the given usersgroup
        ///</summary>
        ///<param name="usersgroup">The usersgroup. Only permissions directly related to this usergroup 
        /// are taken into account</param>
        ///<param name="operation">The operation</param>
        ///<param name="criteria">The criteria</param>
        void AddPermissionsToQuery(UsersGroup usersgroup, string operation, ICriteria criteria);

		/// <summary>
		/// Adds the permissions to the criteria query.
		/// </summary>
		/// <param name="user">The user.</param>
		/// <param name="criteria">The criteria.</param>
		/// <param name="operation">The operation.</param>
		void AddPermissionsToQuery(IUser user, string operation, DetachedCriteria criteria);

        ///<summary>Adds the permissions to the criteria query for the given usersgroup
        ///</summary>
        ///<param name="usersgroup">The usersgroup. Only permissions directly related to this usergroup
        /// are taken into account</param>
        ///<param name="operation">The operation</param>
        ///<param name="criteria">The criteria</param>        
        void AddPermissionsToQuery(UsersGroup usersgroup, string operation, DetachedCriteria criteria);

		/// <summary>
		/// Determines whether the specified user is allowed to perform the specified 
		/// operation on the entity
		/// </summary>
		/// <typeparam name="TEntity">The type of the entity.</typeparam>
		/// <param name="user">The user.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="operation">The operation.</param>
		/// <returns>
		/// 	<c>true</c> if the specified user is allowed; otherwise, <c>false</c>.
		/// </returns>
		bool IsAllowed<TEntity>(IUser user, TEntity entity, string operation) where TEntity : class;

		/// <summary>
		/// Determines whether the specified user is allowed to perform the
		/// specified operation on the entity.
		/// </summary>
		/// <param name="user">The user.</param>
		/// <param name="operation">The operation.</param>
		/// <returns>
		/// 	<c>true</c> if the specified user is allowed; otherwise, <c>false</c>.
		/// </returns>
		bool IsAllowed(IUser user, string operation);

		/// <summary>
		/// Gets the authorization information for the specified user and operation,
		/// allows to easily understand why a given operation was granted / denied.
		/// </summary>
		/// <param name="user">The user.</param>
		/// <param name="operation">The operation.</param>
		/// <returns></returns>
		AuthorizationInformation GetAuthorizationInformation(IUser user, string operation);

		/// <summary>
		/// Gets the authorization information for the specified user and operation on the 
		/// given entity,  allows to easily understand why a given operation was granted / denied.
		/// </summary>
		/// <typeparam name="TEntity">The type of the entity.</typeparam>
		/// <param name="user">The user.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="operation">The operation.</param>
		/// <returns></returns>
		AuthorizationInformation GetAuthorizationInformation<TEntity>(IUser user, TEntity entity, string operation) where TEntity : class;
	}
}