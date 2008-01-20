namespace Rhino.Security
{
	using System;
	using NHibernate;
	using NHibernate.Expressions;
	using Properties;

	/// <summary>
	/// answer authorization questions as well as enhance Criteria
	/// queries
	/// </summary>
	public class AuthorizationService : IAuthorizationService
	{
		private readonly IAuthorizationEditingService authorizationEditingService;

		private readonly IPermissionsService permissionsService;

		/// <summary>
		/// Initializes a new instance of the <see cref="AuthorizationService"/> class.
		/// </summary>
		/// <param name="permissionsService">The permissions service.</param>
		/// <param name="authorizationEditingService">The authorization editing service.</param>
		public AuthorizationService(IPermissionsService permissionsService, IAuthorizationEditingService authorizationEditingService)
		{
			this.permissionsService = permissionsService;
			this.authorizationEditingService = authorizationEditingService;
		}

		#region IAuthorizationService Members

		/// <summary>
		/// Adds the permissions to the criteria query.
		/// </summary>
		/// <param name="user">The user.</param>
		/// <param name="criteria">The criteria.</param>
		/// <param name="operation">The operation.</param>
		public void AddPermissionsToQuery(IUser user, ICriteria criteria, string operation)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Adds the permissions to query.
		/// </summary>
		/// <param name="user">The user.</param>
		/// <param name="criteria">The criteria.</param>
		/// <param name="operation">The operation.</param>
		public void AddPermissionsToQuery(IUser user, DetachedCriteria criteria, string operation)
		{
			throw new NotImplementedException();
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
		public bool IsAllowed<TEntity>(IUser user, TEntity entity, string operation)
		{
			return false;
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
			AuthorizationInformation info = new AuthorizationInformation();
			Operation op = authorizationEditingService.GetOperationByName(operation);
			if (op == null)
			{
				info.AddDeny(Resources.OperationNotDefined, operation);
				return info;
			}
			Permission[] permissions = permissionsService.GetPermissionsFor(user, operation);
			if (permissions.Length == 0)
			{
				UsersGroup[] groups = authorizationEditingService.GetAssociatedUsersGroupFor(user);

				info.AddDeny(Resources.PermissionForOperationNotGrantedToUser,
				                operation,
				                user.SecurityInfo.Name,
				                Strings.Join(groups)
					);
				return info;
			}
			foreach (Permission permission in permissions)
			{
				AddUserLevelPermissionMessage(operation, info, user, permission);
				AddUserGroupLevelPermissionMessage(operation, info, user, permission);
			}
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
		public AuthorizationInformation GetAuthorizationInformation<TEntity>(IUser user, TEntity entity, string operation)
		{
			throw new NotImplementedException();
		}

		#endregion

		private static void AddUserGroupLevelPermissionMessage(string operation, AuthorizationInformation info, IUser user, Permission permission)
		{
			if (permission.UsersGroup != null)
			{
				if (permission.Allow)
				{
					info.AddAllow(Resources.PermissionGrantedForUsersGroup,
					                operation,
					                permission.UsersGroup.Name,
					                GetPermissionTarget(permission),
					                user.SecurityInfo.Name,
					                permission.Level);
				}
				else
				{
					info.AddDeny(Resources.PermissionDeniedForUsersGroup,
					                operation,
					                permission.UsersGroup.Name,
					                GetPermissionTarget(permission),
					                user.SecurityInfo.Name,
					                permission.Level);
				}
			}
		}

		private static void AddUserLevelPermissionMessage(string operation, AuthorizationInformation info, IUser user, Permission permission)
		{
			if (permission.User != null)
			{
				if (permission.Allow)
				{
					info.AddAllow(Resources.PermissionGrantedForUser,
					                operation,
					                user.SecurityInfo.Name,
					                GetPermissionTarget(permission),
					                permission.Level);
				}
				else
				{
					info.AddDeny(Resources.PermissionDeniedForUser,
					                operation,
					                user.SecurityInfo.Name,
					                GetPermissionTarget(permission),
					                permission.Level);
				}
			}
		}

		private static string GetPermissionTarget(Permission permission)
		{
			if (permission.EntitiesGroup != null)
				return permission.EntitiesGroup.Name;
			if (permission.EntitySecurityKey != null)
				return string.Format(Resources.EntityWithKey, permission.EntitySecurityKey.Value);
			return Resources.Everything;
		}
	}
}