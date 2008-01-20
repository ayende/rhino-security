namespace Rhino.Security
{
	using System;
	using System.Collections.Generic;
	using Commons;
	using NHibernate.Expressions;

	/// <summary>
	/// Allow to retrieve and remove permissions
	/// on users, user groups, entities groups and entities.
	/// </summary>
	public class PermissionsService : IPermissionsService
	{
		private readonly IAuthorizationEditingService authorizationEditingService;
		private readonly IRepository<Permission> permissionRepository;

		/// <summary>
		/// Initializes a new instance of the <see cref="PermissionsService"/> class.
		/// </summary>
		/// <param name="authorizationEditingService">The authorization editing service.</param>
		/// <param name="permissionRepository">The permission repository.</param>
		public PermissionsService(IAuthorizationEditingService authorizationEditingService,
								  IRepository<Permission> permissionRepository)
		{
			this.authorizationEditingService = authorizationEditingService;
			this.permissionRepository = permissionRepository;
		}

		#region IPermissionsService Members

		/// <summary>
		/// Gets the permissions for the specified user
		/// </summary>
		/// <param name="user">The user.</param>
		/// <returns></returns>
		public Permission[] GetPermissionsFor(IUser user)
		{
			UsersGroup[] groups = authorizationEditingService.GetAssociatedUsersGroupFor(user);

			DetachedCriteria criteria = DetachedCriteria.For<Permission>()
				.Add(Expression.Eq("User", user) || Expression.In("UsersGroup", groups));

			return FindResults(criteria);
		}


		/// <summary>
		/// Gets the permissions for the specified etntity
		/// </summary>
		/// <param name="user">The user.</param>
		/// <param name="operationName">Name of the operation.</param>
		/// <returns></returns>
		public Permission[] GetPermissionsFor(IUser user, string operationName)
		{
			UsersGroup[] groups = authorizationEditingService.GetAssociatedUsersGroupFor(user);
			string[] operationNames = Strings.GetHierarchicalOperationNames(operationName);
			DetachedCriteria criteria = DetachedCriteria.For<Permission>()
				.Add(Expression.Eq("User", user) || Expression.In("UsersGroup", groups))
				.CreateAlias("Operation", "op")
				.Add(Expression.In("op.Name", operationNames));

			return FindResults(criteria);
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
			EntitiesGroup[] entitiesGroups = authorizationEditingService.GetAssociatedEntitiesGroupsFor(entity);
			UsersGroup[] usersGroups = authorizationEditingService.GetAssociatedUsersGroupFor(user);

			DetachedCriteria criteria = DetachedCriteria.For<Permission>()
				.Add(Expression.Eq("User", user) || Expression.In("UsersGroup", usersGroups))
				.Add(Expression.Eq("EntitySecurityKey", key) || Expression.In("EntitiesGroup", entitiesGroups));

			return FindResults(criteria);
		}


		/// <summary>
		/// Gets the permissions for the specified etntity
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
			EntitiesGroup[] entitiesGroups = authorizationEditingService.GetAssociatedEntitiesGroupsFor(entity);
			UsersGroup[] usersGroups = authorizationEditingService.GetAssociatedUsersGroupFor(user);

			DetachedCriteria criteria = DetachedCriteria.For<Permission>()
				.Add(Expression.Eq("User", user) || Expression.In("UsersGroup", usersGroups))
				.Add(Expression.Eq("EntitySecurityKey", key) || Expression.In("EntitiesGroup", entitiesGroups))
			 .CreateAlias("Operation", "op")
				.Add(Expression.In("op.Name", operationNames));

			return FindResults(criteria);
		}

		/// <summary>
		/// Gets the permissions for the specified etntity
		/// </summary>
		/// <typeparam name="TEntity">The type of the entity.</typeparam>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		public Permission[] GetPermissionsFor<TEntity>(TEntity entity) where TEntity : class
		{
			if (entity is IUser)// the combpiler will direct IUser instance to here, annoying
				return GetPermissionsFor((IUser)entity);

			Guid key = Security.ExtractKey(entity);
			EntitiesGroup[] groups = authorizationEditingService.GetAssociatedEntitiesGroupsFor(entity);
			DetachedCriteria criteria = DetachedCriteria.For<Permission>()
				.Add(Expression.Eq("EntitySecurityKey", key) || Expression.In("EntitiesGroup", groups));

			return FindResults(criteria);
		}

		#endregion

		private Permission[] FindResults(DetachedCriteria criteria)
		{
			ICollection<Permission> permissions = permissionRepository.FindAll(criteria,
				Order.Desc("Level"), Order.Asc("Allow"));
			return Collection.ToArray<Permission>(permissions);
		}
	}
}