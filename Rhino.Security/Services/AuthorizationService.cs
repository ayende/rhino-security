using System;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using Rhino.Security.Impl.Util;
using Rhino.Security.Interfaces;
using Rhino.Security.Model;
using Rhino.Security.Properties;
using System.Linq;
using NHibernate.Linq;
using LinqExpr = System.Linq.Expressions.Expression;
using LinqExprs = System.Linq.Expressions;

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

        private static readonly System.Reflection.PropertyInfo getQueryProviderSession =
            typeof(DefaultQueryProvider).GetProperty("Session", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

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
            ICriterion allowed = GetPermissionQueryInternal(user, operation, GetSecurityKeyProperty(criteria));
			criteria.Add(allowed);
		}

        ///<summary>
        /// Adds the permissions to the criteria query for the given usersgroup
        ///</summary>
        ///<param name="usersgroup">The usersgroup. Only permissions directly related to this usergroup
        /// are taken into account</param>
        ///<param name="operation">The operation</param>
        ///<param name="criteria">The criteria</param>
        public void AddPermissionsToQuery(UsersGroup usersgroup,string operation, ICriteria criteria)
        {            
            ICriterion allowed = GetPermissionQueryInternal(usersgroup, operation, GetSecurityKeyProperty(criteria));
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
            ICriterion allowed = GetPermissionQueryInternal(user, operation, GetSecurityKeyProperty(criteria));
			criteria.Add(allowed);
		}

        ///<summary>Adds the permissions to the criteria query for the given usersgroup
        ///</summary>
        ///<param name="usersgroup">The usersgroup. Only permissions directly related to this usergroup
        /// are taken into account</param>
        ///<param name="operation">The operation</param>
        ///<param name="criteria">The criteria</param>
        public void AddPermissionsToQuery(UsersGroup usersgroup, string operation, DetachedCriteria criteria)
        {            
            ICriterion allowed = GetPermissionQueryInternal(usersgroup, operation, GetSecurityKeyProperty(criteria));
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
			Permission[] permissions = permissionsService.GetGlobalPermissionsFor(user, operation);
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
			Permission[] permissions = permissionsService.GetGlobalPermissionsFor(user, operation);
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

		private static ICriterion GetPermissionQueryInternal(IUser user, string operation, string securityKeyProperty)
		{
			string[] operationNames = Strings.GetHierarchicalOperationNames(operation);
			DetachedCriteria criteria = DetachedCriteria.For<Permission>("permission")
				.CreateAlias("Operation", "op")
				.CreateAlias("EntitiesGroup", "entityGroup", JoinType.LeftOuterJoin)
				.CreateAlias("entityGroup.Entities", "entityKey", JoinType.LeftOuterJoin)
				.SetProjection(Projections.Property("Allow"))
				.Add(Restrictions.In("op.Name", operationNames))
				.Add(Restrictions.Eq("User.id", user.SecurityInfo.Identifier) 
				|| Subqueries.PropertyIn("UsersGroup.Id", 
										 SecurityCriterions.AllGroups(user).SetProjection(Projections.Id())))
				.Add(
				Property.ForName(securityKeyProperty).EqProperty("permission.EntitySecurityKey") ||
				Property.ForName(securityKeyProperty).EqProperty("entityKey.EntitySecurityKey") ||
				(
					Restrictions.IsNull("permission.EntitySecurityKey") &&
					Restrictions.IsNull("permission.EntitiesGroup")
				)
				)
				.SetMaxResults(1)
				.AddOrder(Order.Desc("Level"))
				.AddOrder(Order.Asc("Allow"));
			return Subqueries.Eq(true, criteria);
		}

        private ICriterion GetPermissionQueryInternal(UsersGroup usersgroup, string operation, string securityKeyProperty)
        {
            string[] operationNames = Strings.GetHierarchicalOperationNames(operation);
            DetachedCriteria criteria = DetachedCriteria.For<Permission>("permission")
                .CreateAlias("Operation", "op")
                .CreateAlias("EntitiesGroup", "entityGroup", JoinType.LeftOuterJoin)
                .CreateAlias("entityGroup.Entities", "entityKey", JoinType.LeftOuterJoin)
                .SetProjection(Projections.Property("Allow"))
                .Add(Expression.In("op.Name", operationNames))
                .Add(Expression.Eq("UsersGroup", usersgroup))
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

        private static IQueryable<TEntity> GetPermissionQueryInternal<TEntity>(IUser user, string operation, IQueryable<TEntity> query) where TEntity : class
        {
            var nhQuery = (NhQueryable<TEntity>)query;
            var nhQueryProvider = (DefaultQueryProvider)nhQuery.Provider;
            var session = (ISession)getQueryProviderSession.GetValue(nhQueryProvider, null);

            var securityKeyProperty = Security.GetSecurityKeyProperty(typeof(TEntity));

            string[] operationNames = Strings.GetHierarchicalOperationNames(operation);
            var userGroupIds = SecurityCriterions.AllGroups(user, session).Select(x => x.Id);

            var entityParam = LinqExpr.Parameter(typeof(TEntity), "e");
            var entityRefParam = LinqExpr.Parameter(typeof(EntityReference), "er");
            var permParam = LinqExpr.Parameter(typeof(Permission), "p");

            // .Any<EntityReference>(...)
            var anyEntityRef = SecurityCriterions.anyFunc.MakeGenericMethod(typeof(EntityReference));

            // p.EntitySecurityKey == e.SecurityKey
            var isEqPermSecKey = LinqExpr.Equal(LinqExpr.PropertyOrField(permParam, "EntitySecurityKey"), LinqExpr.Convert(LinqExpr.PropertyOrField(entityParam, securityKeyProperty), typeof(Guid?)));

            // er.EntitySecurityKey == e.SecurityKey
            var isEqEntityRefSecKey = LinqExpr.Equal(LinqExpr.PropertyOrField(entityRefParam, "EntitySecurityKey"), LinqExpr.PropertyOrField(entityParam, securityKeyProperty));

            // IQueryable.Any<EntityReference>(p.EntitiesGroup.Entities, er => er.EntitySecurityKey == e.SecurityKey)
            var isAnyEqEntitySecKey = LinqExpr.Call(anyEntityRef, LinqExpr.PropertyOrField(LinqExpr.PropertyOrField(permParam, "EntitiesGroup"), "Entities"),
                LinqExpr.Lambda(isEqEntityRefSecKey, entityRefParam));

            // p.EntitySecurityKey==(Guid?)null && p.EntitiesGroup==null
            var isNullSecKeyOrGroup = LinqExpr.AndAlso(
                LinqExpr.Equal(LinqExpr.PropertyOrField(permParam, "EntitySecurityKey"), LinqExpr.Convert(LinqExpr.Constant(null), typeof(Guid?))),
                LinqExpr.ReferenceEqual(LinqExpr.PropertyOrField(permParam, "EntitiesGroup"), LinqExpr.Constant(null)));

            // (p.EntitySecurityKey==(Guid?)null && p.EntitiesGroup==null)
            // || p.EntitySecurityKey == e.SecurityKey
            // || IQueryable.Any<EntityReference>(p.EntitiesGroup.Entities, er => er.EntitySecurityKey == e.SecurityKey)
            var isPermMatch = LinqExpr.OrElse(isNullSecKeyOrGroup, LinqExpr.OrElse(isEqPermSecKey, isAnyEqEntitySecKey));

            // p.User.Id == user.SecurityInfo.Identifier
            var userIdPropName = Security.GetUserTypeIdPropertyName(session);
            var isSameUserId = LinqExpr.Equal(LinqExpr.PropertyOrField(LinqExpr.Convert(LinqExpr.PropertyOrField(permParam, "User"), Security.UserType), userIdPropName), LinqExpr.Constant(user.SecurityInfo.Identifier));

            // userGroupIds.Contains(p.UsersGroup.Id)
            var containsGuid = SecurityCriterions.containsFunc.MakeGenericMethod(typeof(Guid));
            var isInUsersGroup = LinqExpr.Call(containsGuid, LinqExpr.Constant(userGroupIds), LinqExpr.PropertyOrField(LinqExpr.PropertyOrField(permParam, "UsersGroup"), "Id"));

            // p => (p.User.Id == user.SecurityInfo.Identifier) || userGroupIds.Contains(p.UsersGroup.Id)
            var isSameUserOrInUserGroup = LinqExpr.Lambda<Func<Permission, bool>>(
                LinqExpr.OrElse(isSameUserId, isInUsersGroup),
                permParam);

            LinqExprs.Expression<Func<bool>> isPermAllowed = () => true == session.Query<Permission>()
                .Where(p => operationNames.Contains(p.Operation.Name))
                .Where(isSameUserOrInUserGroup)
                .Where(LinqExpr.Lambda<Func<Permission, bool>>(isPermMatch, permParam))
                .OrderByDescending(p => p.Level)
                .ThenBy(p => p.Allow)
                .Select(p => p.Allow)
                .FirstOrDefault();

            query = query.Where(LinqExpr.Lambda<Func<TEntity, bool>>(isPermAllowed.Body, entityParam));

            return query;
        }

        private static IQueryable<TEntity> GetPermissionQueryInternal<TEntity>(UsersGroup usersgroup, string operation, IQueryable<TEntity> query) where TEntity : class
        {
            var nhQuery = (NhQueryable<TEntity>)query;
            var nhQueryProvider = (DefaultQueryProvider)nhQuery.Provider;
            var session = (ISession)getQueryProviderSession.GetValue(nhQueryProvider, null);

            var securityKeyProperty = Security.GetSecurityKeyProperty(typeof(TEntity));

            string[] operationNames = Strings.GetHierarchicalOperationNames(operation);

            var entityParam = LinqExpr.Parameter(typeof(TEntity), "e");
            var entityRefParam = LinqExpr.Parameter(typeof(EntityReference), "er");
            var permParam = LinqExpr.Parameter(typeof(Permission), "p");

            // .Any<EntityReference>(...)
            var anyEntityRef = SecurityCriterions.anyFunc.MakeGenericMethod(typeof(EntityReference));

            // p.EntitySecurityKey == e.SecurityKey
            var isEqPermSecKey = LinqExpr.Equal(LinqExpr.PropertyOrField(permParam, "EntitySecurityKey"), LinqExpr.Convert(LinqExpr.PropertyOrField(entityParam, securityKeyProperty), typeof(Guid?)));

            // er.EntitySecurityKey == e.SecurityKey
            var isEqEntityRefSecKey = LinqExpr.Equal(LinqExpr.PropertyOrField(entityRefParam, "EntitySecurityKey"), LinqExpr.PropertyOrField(entityParam, securityKeyProperty));

            // IQueryable.Any<EntityReference>(p.EntitiesGroup.Entities, er => er.EntitySecurityKey == e.SecurityKey)
            var isAnyEqEntitySecKey = LinqExpr.Call(anyEntityRef, LinqExpr.PropertyOrField(LinqExpr.PropertyOrField(permParam, "EntitiesGroup"), "Entities"),
                LinqExpr.Lambda(isEqEntityRefSecKey, entityRefParam));

            // p.EntitySecurityKey==(Guid?)null && p.EntitiesGroup==null
            var isNullSecKeyOrGroup = LinqExpr.AndAlso(
                LinqExpr.Equal(LinqExpr.PropertyOrField(permParam, "EntitySecurityKey"), LinqExpr.Convert(LinqExpr.Constant(null), typeof(Guid?))),
                LinqExpr.ReferenceEqual(LinqExpr.PropertyOrField(permParam, "EntitiesGroup"), LinqExpr.Constant(null)));

            // (p.EntitySecurityKey==(Guid?)null && p.EntitiesGroup==null)
            // || p.EntitySecurityKey == e.SecurityKey
            // || IQueryable.Any<EntityReference>(p.EntitiesGroup.Entities, er => er.EntitySecurityKey == e.SecurityKey)
            var isPermMatch = LinqExpr.OrElse(isNullSecKeyOrGroup, LinqExpr.OrElse(isEqPermSecKey, isAnyEqEntitySecKey));

            LinqExprs.Expression<Func<bool>> isPermAllowed = () => true == session.Query<Permission>()
                .Where(p => operationNames.Contains(p.Operation.Name))
                .Where(p => p.UsersGroup == usersgroup)
                .Where(LinqExpr.Lambda<Func<Permission, bool>>(isPermMatch, permParam))
                .OrderByDescending(p => p.Level)
                .ThenBy(p => p.Allow)
                .Select(p => p.Allow)
                .FirstOrDefault();

            query = query.Where(LinqExpr.Lambda<Func<TEntity, bool>>(isPermAllowed.Body, entityParam));

            return query;
        }

        private string GetSecurityKeyProperty(DetachedCriteria criteria)
        {
            Type rootType = criteria.GetRootEntityTypeIfAvailable();
            return criteria.Alias + "." + Security.GetSecurityKeyProperty(rootType);
        }

        private string GetSecurityKeyProperty(ICriteria criteria)
        {
            Type rootType = criteria.GetRootEntityTypeIfAvailable();
            return criteria.Alias + "." + Security.GetSecurityKeyProperty(rootType);
        }

        private string GetSecurityKeyProperty<TEntity>(IQueryable<TEntity> query) where TEntity : class
        {
            var rootType = typeof(TEntity);
            return Security.GetSecurityKeyProperty(rootType);
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

        /// <summary>
        /// Adds the permissions to the linq query.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="user">The user.</param>
        /// <param name="operation">The operation.</param>
        /// <param name="query">The linq query.</param>
        /// <returns>The modified linq query.</returns>
        public IQueryable<TEntity> AddPermissionsToQuery<TEntity>(IUser user, string operation, IQueryable<TEntity> query) where TEntity : class
        {
            return GetPermissionQueryInternal<TEntity>(user, operation, query);
        }

        /// <summary>
        /// Adds the permissions to the linq query.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        ///<param name="usersgroup">The usersgroup. Only permissions directly related to this usergroup
        /// are taken into account</param>
        /// <param name="operation">The operation.</param>
        /// <param name="query">The linq query.</param>
        /// <returns>The modified linq query.</returns>
        public IQueryable<TEntity> AddPermissionsToQuery<TEntity>(UsersGroup usersgroup, string operation, IQueryable<TEntity> query) where TEntity : class
        {
            return GetPermissionQueryInternal<TEntity>(usersgroup, operation, query);
        }
    }
}
