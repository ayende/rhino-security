using System;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using Rhino.Commons;
using Rhino.Security.Impl.Util;
using Rhino.Security.Interfaces;
using Rhino.Security.Model;
using Rhino.Security.Properties;

namespace Rhino.Security.Services
{
	/// <summary>
	/// Answers authorization questions as well as enhance Criteria
	/// queries
	/// </summary>
	public class AuthorizationService : IAuthorizationService
	{
		private readonly IAuthorizationRepository authorizationRepository;

		private readonly IPermissionsService permissionsService;

		/// <summary>
		/// Initializes a new instance of the <see cref="AuthorizationService"/> class.
		/// </summary>
		/// <param name="permissionsService">The permissions service.</param>
		/// <param name="authorizationRepository">The authorization editing service.</param>
		public AuthorizationService(IPermissionsService permissionsService,
		                            IAuthorizationRepository authorizationRepository)
		{
			this.permissionsService = permissionsService;
			this.authorizationRepository = authorizationRepository;
		}

		#region IAuthorizationService Members

		/// <summary>
		/// Adds the permissions to the criteria query.
		/// </summary>
		/// <param name="user">The user.</param>
		/// <param name="criteria">The criteria.</param>
		/// <param name="operation">The operation.</param>
		public void AddPermissionsToQuery(IUser user, string operation, ICriteria criteria)
		{
			Type rootType = Rhino.Commons.NHibernate.CriteriaUtil.GetRootType(criteria);
			string securityKeyProperty = criteria.Alias+"." + Security.GetSecurityKeyProperty(rootType);
			ICriterion allowed = GetPermissionQueryInternal(user, operation, securityKeyProperty);
			criteria.Add(allowed);
		}

		/// <summary>
		/// Adds the permissions to query.
		/// </summary>
		/// <param name="user">The user.</param>
		/// <param name="criteria">The criteria.</param>
		/// <param name="operation">The operation.</param>
		public void AddPermissionsToQuery(IUser user, string operation, DetachedCriteria criteria)
		{
			Type rootType = Commons.NHibernate.CriteriaUtil.GetRootType(criteria, UnitOfWork.CurrentSession);
			string securityKeyProperty = criteria.Alias + "." + Security.GetSecurityKeyProperty(rootType);
			ICriterion allowed = GetPermissionQueryInternal(user, operation, securityKeyProperty);
			criteria.Add(allowed);
		}

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
		public bool IsAllowed<TEntity>(IUser user, TEntity entity, string operation) where TEntity : class
		{
			Permission[] permissions = permissionsService.GetPermissionsFor(user, entity, operation);
			if (permissions.Length == 0)
				return false;
			return permissions[0].Allow;
		}

		/// <summary>
		/// Determines whether the specified user is allowed to perform the
		/// specified operation on the entity.
		/// </summary>
		/// <param name="user">The user.</param>
		/// <param name="operation">The operation.</param>
		/// <returns>
		/// 	<c>true</c> if the specified user is allowed; otherwise, <c>false</c>.
		/// </returns>
		public bool IsAllowed(IUser user, string operation)
		{
			Permission[] permissions = permissionsService.GetPermissionsFor(user, operation);
			if (permissions.Length == 0)
				return false;
			return permissions[0].Allow;
		}

		/// <summary>
		/// Gets the authorization information for the specified user and operation,
		/// allows to easily understand why a given operation was granted / denied.
		/// </summary>
		/// <param name="user">The user.</param>
		/// <param name="operation">The operation.</param>
		/// <returns></returns>
		public AuthorizationInformation GetAuthorizationInformation(IUser user, string operation)
		{
			AuthorizationInformation info;
			if (InitializeAuthorizationInfo(operation, out info))
				return info;
			Permission[] permissions = permissionsService.GetPermissionsFor(user, operation);
			AddPermissionDescriptionToAuthorizationInformation<object>(operation, info, user, permissions, null);
			return info;
		}

		/// <summary>
		/// Gets the authorization information for the specified user and operation on the
		/// given entity,  allows to easily understand why a given operation was granted / denied.
		/// </summary>
		/// <typeparam name="TEntity">The type of the entity.</typeparam>
		/// <param name="user">The user.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="operation">The operation.</param>
		/// <returns></returns>
		public AuthorizationInformation GetAuthorizationInformation<TEntity>(IUser user, TEntity entity,
		                                                                     string operation) where TEntity : class
		{
			AuthorizationInformation info;
			if (InitializeAuthorizationInfo(operation, out info))
				return info;
			Permission[] permissions = permissionsService.GetPermissionsFor(user, entity, operation);
			AddPermissionDescriptionToAuthorizationInformation(operation, info, user, permissions, entity);
			return info;
		}

		#endregion

		private ICriterion GetPermissionQueryInternal(IUser user, string operation, string securityKeyProperty)
		{
			string[] operationNames = Strings.GetHierarchicalOperationNames(operation);
			DetachedCriteria criteria = DetachedCriteria.For<Permission>("permission")
				.CreateAlias("Operation", "op")
				.CreateAlias("EntitiesGroup", "entityGroup", JoinType.LeftOuterJoin)
				.CreateAlias("entityGroup.Entities", "entityKey", JoinType.LeftOuterJoin)
				.SetProjection(Projections.Property("Allow"))
				.Add(Expression.In("op.Name", operationNames))
				.Add(Expression.Eq("User", user) 
				|| Subqueries.PropertyIn("UsersGroup.Id", 
										 SecurityCriterions.AllGroups(user).SetProjection(Projections.Id())))
				.Add(
				Property.ForName(securityKeyProperty).EqProperty("permission.EntitySecurityKey") ||
				Property.ForName(securityKeyProperty).EqProperty("entityKey.EntitySecurityKey") ||
				(
					Expression.IsNull("permission.EntitySecurityKey") &&
					Expression.IsNull("permission.EntitiesGroup")
				)
				)
				.SetMaxResults(1)
				.AddOrder(Order.Desc("Level"))
				.AddOrder(Order.Asc("Allow"));
			return Subqueries.Eq(true, criteria);
		}

		private void AddPermissionDescriptionToAuthorizationInformation<TEntity>(string operation,
		                                                                         AuthorizationInformation info,
		                                                                         IUser user, Permission[] permissions,
		                                                                         TEntity entity)
			where TEntity : class
		{
			string entityDescription = "";
			string entitiesGroupsDescription = "";
			if (entity != null)
			{
				EntitiesGroup[] entitiesGroups = authorizationRepository.GetAssociatedEntitiesGroupsFor(entity);
				entityDescription = Security.GetDescription(entity);
				entitiesGroupsDescription = Strings.Join(entitiesGroups);
			}
			if (permissions.Length == 0)
			{
				UsersGroup[] usersGroups = authorizationRepository.GetAssociatedUsersGroupFor(user);

				if (entity == null) //not on specific entity
				{
					info.AddDeny(Resources.PermissionForOperationNotGrantedToUser,
					             operation,
					             user.SecurityInfo.Name,
					             Strings.Join(usersGroups)
						);
				}
				else
				{
					info.AddDeny(Resources.PermissionForOperationNotGrantedToUserOnEntity,
					             operation,
					             user.SecurityInfo.Name,
					             Strings.Join(usersGroups),
					             entityDescription,
					             entitiesGroupsDescription);
				}
				return;
			}
			foreach (Permission permission in permissions)
			{
				AddUserLevelPermissionMessage(operation, info, user, permission, entityDescription,
				                              entitiesGroupsDescription);
				AddUserGroupLevelPermissionMessage(operation, info, user, permission, entityDescription,
				                                   entitiesGroupsDescription);
			}
		}

		private bool InitializeAuthorizationInfo(string operation, out AuthorizationInformation info)
		{
			info = new AuthorizationInformation();
			Operation op = authorizationRepository.GetOperationByName(operation);
			if (op == null)
			{
				info.AddDeny(Resources.OperationNotDefined, operation);
				return true;
			}
			return false;
		}

		private void AddUserGroupLevelPermissionMessage(string operation, AuthorizationInformation info,
		                                                IUser user, Permission permission,
		                                                string entityDescription,
		                                                string entitiesGroupsDescription)
		{
			if (permission.UsersGroup != null)
			{
				UsersGroup[] ancestryAssociation =
					authorizationRepository.GetAncestryAssociation(user, permission.UsersGroup.Name);
				string groupAncestry = Strings.Join(ancestryAssociation, " -> ");
				if (permission.Allow)
				{
					info.AddAllow(Resources.PermissionGrantedForUsersGroup,
					              operation,
					              permission.UsersGroup.Name,
					              GetPermissionTarget(permission, entityDescription, entitiesGroupsDescription),
					              user.SecurityInfo.Name,
					              permission.Level,
					              groupAncestry);
				}
				else
				{
					info.AddDeny(Resources.PermissionDeniedForUsersGroup,
					             operation,
					             permission.UsersGroup.Name,
					             GetPermissionTarget(permission, entityDescription, entitiesGroupsDescription),
					             user.SecurityInfo.Name,
					             permission.Level,
					             groupAncestry);
				}
			}
		}

		private static void AddUserLevelPermissionMessage(
			string operation,
			AuthorizationInformation info,
			IUser user,
			Permission permission,
			string entityDescription,
			string entitiesGroupsDescription)
		{
			if (permission.User != null)
			{
				string target = GetPermissionTarget(permission, entityDescription, entitiesGroupsDescription);
				if (permission.Allow)
				{
					info.AddAllow(Resources.PermissionGrantedForUser,
					              operation,
					              user.SecurityInfo.Name,
					              target,
					              permission.Level);
				}
				else
				{
					info.AddDeny(Resources.PermissionDeniedForUser,
					             operation,
					             user.SecurityInfo.Name,
					             target,
					             permission.Level);
				}
			}
		}

		private static string GetPermissionTarget(Permission permission, string entityDescription,
		                                          string entitiesGroupsDescription)
		{
			if (permission.EntitiesGroup != null)
			{
				if (string.IsNullOrEmpty(entitiesGroupsDescription) == false)
				{
					return string.Format(Resources.EntityWithGroups,
					                     permission.EntitiesGroup.Name,
					                     entityDescription, entitiesGroupsDescription);
				}
				else
				{
					return permission.EntitiesGroup.Name;
				}
			}
			if (permission.EntitySecurityKey != null)
				return entityDescription;
			return Resources.Everything;
		}
	}
}
