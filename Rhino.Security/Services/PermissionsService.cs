using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using Rhino.Security.Impl.Util;
using Rhino.Security.Interfaces;
using Rhino.Security.Model;
using System.Linq;

namespace Rhino.Security.Services
{
    /// <summary>
	/// Allow to retrieve and remove permissions
	/// on users, user groups, entities groups and entities.
	/// </summary>
	public class PermissionsService : IPermissionsService
	{
		private readonly IAuthorizationRepository authorizationRepository;
        private readonly ISession session;

        /// <summary>
		/// Initializes a new instance of the <see cref="PermissionsService"/> class.
		/// </summary>
		/// <param name="authorizationRepository">The authorization editing service.</param>
		/// <param name="session">The NHibernate session</param>
		public PermissionsService(IAuthorizationRepository authorizationRepository,
		                          ISession session)
		{
			this.authorizationRepository = authorizationRepository;
		    this.session = session;
		}

		#region IPermissionsService Members

		/// <summary>
		/// Gets the permissions for the specified user
		/// </summary>
		/// <param name="user">The user.</param>
		/// <returns></returns>
		public Permission[] GetPermissionsFor(IUser user)
		{
			DetachedCriteria criteria = DetachedCriteria.For<Permission>()
				.Add(Expression.Eq("User", user)
				     || Subqueries.PropertyIn("UsersGroup.Id",
				                              SecurityCriterions.AllGroups(user).SetProjection(Projections.Id())));

			return FindResults(criteria);
		}


		/// <summary>
		/// Gets the permissions for the specified entity
		/// </summary>
		/// <param name="user">The user.</param>
		/// <param name="operationName">Name of the operation.</param>
		/// <returns></returns>
		public Permission[] GetGlobalPermissionsFor(IUser user, string operationName)
		{
			string[] operationNames = Strings.GetHierarchicalOperationNames(operationName);
			DetachedCriteria criteria = DetachedCriteria.For<Permission>()
				.Add(Expression.Eq("User", user)
				     || Subqueries.PropertyIn("UsersGroup.Id",
				                              SecurityCriterions.AllGroups(user).SetProjection(Projections.Id())))
                .Add(Expression.IsNull("EntitiesGroup"))
                .Add(Expression.IsNull("EntitySecurityKey"))
                .CreateAlias("Operation", "op")
				.Add(Expression.In("op.Name", operationNames));

			return FindResults(criteria);
		}

        /// <summary>
        /// Gets all permissions for the specified operation
        /// </summary>
        /// <param name="operationName">Name of the operation.</param>
        /// <returns></returns>
        public Permission[] GetPermissionsFor(string operationName)
        {
            string[] operationNames = Strings.GetHierarchicalOperationNames(operationName);
            DetachedCriteria criteria = DetachedCriteria.For<Permission>()
                .CreateAlias("Operation", "op")
                .Add(Restrictions.In("op.Name", operationNames));

            return this.FindResults(criteria);
        }

        /// <summary>
		/// Gets the permissions for the specified user and entity
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="user">The user.</param>
		/// <param name="entity"></param>
		/// <returns></returns>
		public Permission[] GetPermissionsFor<TEntity>(IUser user, TEntity entity) where TEntity : class
		{
			Guid key = Security.ExtractKey(entity);
			EntitiesGroup[] entitiesGroups = authorizationRepository.GetAssociatedEntitiesGroupsFor(entity);

			DetachedCriteria criteria = DetachedCriteria.For<Permission>()
				.Add(Expression.Eq("User", user)
				     || Subqueries.PropertyIn("UsersGroup.Id",
				                              SecurityCriterions.AllGroups(user).SetProjection(Projections.Id())))
				.Add(Expression.Eq("EntitySecurityKey", key) || Expression.In("EntitiesGroup", entitiesGroups));

			return FindResults(criteria);
		}

		/// <summary>
		/// Gets the permissions for the specified entity
		/// </summary>
		/// <typeparam name="TEntity">The type of the entity.</typeparam>
		/// <param name="user">The user.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="operationName">Name of the operation.</param>
		/// <returns></returns>
		public Permission[] GetPermissionsFor<TEntity>(IUser user, TEntity entity, string operationName) where TEntity : class
		{
			Guid key = Security.ExtractKey(entity);
			string[] operationNames = Strings.GetHierarchicalOperationNames(operationName);
			EntitiesGroup[] entitiesGroups = authorizationRepository.GetAssociatedEntitiesGroupsFor(entity);

			//UsersGroup[] usersGroups = authorizationRepository.GetAssociatedUsersGroupFor(user);					

			AbstractCriterion onCriteria =
				(Restrictions.Eq("EntitySecurityKey", key) || Restrictions.In("EntitiesGroup", entitiesGroups)) ||
				(Restrictions.IsNull("EntitiesGroup") && Restrictions.IsNull("EntitySecurityKey"));
			DetachedCriteria criteria = DetachedCriteria.For<Permission>()
				.Add(Restrictions.Eq("User", user)
				     || Subqueries.PropertyIn("UsersGroup.Id",
				                              SecurityCriterions.AllGroups(user).SetProjection(Projections.Id())))
				.Add(onCriteria)
				.CreateAlias("Operation", "op")
				.Add(Restrictions.In("op.Name", operationNames));

			return FindResults(criteria);
		}

		/// <summary>
		/// Gets the permissions for the specified entity
		/// </summary>
		/// <typeparam name="TEntity">The type of the entity.</typeparam>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		public Permission[] GetPermissionsFor<TEntity>(TEntity entity) where TEntity : class
		{
			if (entity is IUser) // the combpiler will direct IUser instance to here, annoying
				return GetPermissionsFor((IUser) entity);

			Guid key = Security.ExtractKey(entity);
			EntitiesGroup[] groups = authorizationRepository.GetAssociatedEntitiesGroupsFor(entity);
			DetachedCriteria criteria = DetachedCriteria.For<Permission>()
				.Add(Expression.Eq("EntitySecurityKey", key) || Expression.In("EntitiesGroup", groups));

			return FindResults(criteria);
		}

		#endregion

		private Permission[] FindResults(DetachedCriteria criteria)
		{
		    ICollection<Permission> permissions = criteria.GetExecutableCriteria(session)
		        .AddOrder(Order.Desc("Level"))
		        .AddOrder(Order.Asc("Allow"))
                .SetCacheable(true)
                .List<Permission>();
		    return permissions.ToArray();
		}
	}
}
