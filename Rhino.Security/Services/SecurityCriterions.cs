using System;

namespace Rhino.Security.Services
{
	using Model;
	using NHibernate.Criterion;
	using NHibernate.SqlCommand;
    using NHibernate.Linq;
    using System.Linq;
    using System.Collections.Generic;
    using LinqExpr = System.Linq.Expressions.Expression;

	/// <summary>
	/// A factory for common DetachedCriteria.
	/// </summary>
	internal static class SecurityCriterions
	{
        internal static readonly System.Reflection.MethodInfo anyFunc = 
            typeof(System.Linq.Enumerable).GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
                .Single(x => x.Name == "Any" && x.GetParameters().Length == 2);

        internal static readonly System.Reflection.MethodInfo containsFunc =
            typeof(System.Linq.Enumerable).GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
                .Single(x => x.Name == "Contains" && x.GetParameters().Length == 2);

		public static DetachedCriteria DirectUsersGroups(IUser user)
		{
			return DetachedCriteria.For<UsersGroup>()
				.CreateAlias("Users", "user")
				.Add(Expression.Eq("user.id", user.SecurityInfo.Identifier));
		}

        public static IQueryable<UsersGroup> DirectUsersGroups(IUser user, NHibernate.ISession session)
        {
            var userAnyFunc = anyFunc.MakeGenericMethod(Security.UserType);

            var userIdPropName = Security.GetUserTypeIdPropertyName(session);
            var lambdaParamUser = LinqExpr.Parameter(Security.UserType, "u");

            // u => u.Id == user.SecurityInfo.Identifier
            var isSameId = LinqExpr.Lambda(
                LinqExpr.Equal(LinqExpr.PropertyOrField(lambdaParamUser, userIdPropName), LinqExpr.Constant(user.SecurityInfo.Identifier)),
                lambdaParamUser);

            var lambdaParamUsersGroup = LinqExpr.Parameter(typeof(UsersGroup), "ug");

            // ((ICollection<TUser>)ug.Users).Any(u => u.Id == user.SecurityInfo.Identifier)
            var usersCollection = LinqExpr.Convert(LinqExpr.PropertyOrField(lambdaParamUsersGroup, "Users"), typeof(System.Collections.Generic.ICollection<>).MakeGenericType(Security.UserType));
            var isUserInGroup = LinqExpr.Call(userAnyFunc, usersCollection, isSameId);

            var cond = LinqExpr.Lambda<Func<UsersGroup, bool>>(isUserInGroup, lambdaParamUsersGroup);

            var query = session.Query<UsersGroup>()
                .Where(cond);

            return query;
        }

        public static DetachedCriteria DirectEntitiesGroups<TEntity>(TEntity entity) where TEntity : class
        {
            Guid key = Security.ExtractKey(entity);
            return DetachedCriteria.For<EntitiesGroup>()
                .CreateAlias("Entities", "e")
                .Add(Expression.Eq("e.EntitySecurityKey", key));
        }

        public static IQueryable<EntitiesGroup> DirectEntitiesGroups<TEntity>(TEntity entity, NHibernate.ISession session) where TEntity : class
        {
            Guid key = Security.ExtractKey(entity);
            var query = session.Query<EntitiesGroup>()
                .Where(ug => ug.Entities.Any(e => e.EntitySecurityKey == key));
            return query;
        }

		public static DetachedCriteria AllGroups(IUser user)
		{
			DetachedCriteria directGroupsCriteria = DirectUsersGroups(user)
				.SetProjection(Projections.Id());

            DetachedCriteria criteria = DetachedCriteria.For<UsersGroup>()
                                                        .CreateAlias("Users", "user", JoinType.LeftOuterJoin)
                                                        .CreateAlias("AllChildren", "child", JoinType.LeftOuterJoin)
                                                        .Add(
                                                             Subqueries.PropertyIn("child.id", directGroupsCriteria) ||
                                                             Expression.Eq("user.id", user.SecurityInfo.Identifier))
                                        .SetProjection(Projections.Id());

            return DetachedCriteria.For<UsersGroup>()
                                    .Add(Subqueries.PropertyIn("Id", criteria));
		}

        public static IQueryable<UsersGroup> AllGroups(IUser user, NHibernate.ISession session)
        {
            var directGroupIds = DirectUsersGroups(user, session)
                .Select(x => x.Id);

            var userIdPropName = Security.GetUserTypeIdPropertyName(session);
            var userAnyFunc = anyFunc.MakeGenericMethod(Security.UserType);
            var groupAnyFunc = anyFunc.MakeGenericMethod(typeof(UsersGroup));

            var lambdaParamUser = LinqExpr.Parameter(Security.UserType, "u");

            // u => u.Id == user.SecurityInfo.Identifier
            var isSameUserId = LinqExpr.Lambda(
                LinqExpr.Equal(LinqExpr.PropertyOrField(lambdaParamUser, userIdPropName), LinqExpr.Constant(user.SecurityInfo.Identifier)),
                lambdaParamUser);

            System.Linq.Expressions.Expression<Func<UsersGroup, bool>> isInGroupIds = g => directGroupIds.Contains(g.Id);

            var lambdaParamUsersGroup = LinqExpr.Parameter(typeof(UsersGroup), "ug");

            // ((ICollection<TUser>)ug.Users).Any(u => u.Id == user.SecurityInfo.Identifier)
            var usersCollection = LinqExpr.Convert(LinqExpr.PropertyOrField(lambdaParamUsersGroup, "Users"), typeof(ICollection<>).MakeGenericType(Security.UserType));
            var isUserInGroup = LinqExpr.Call(userAnyFunc, usersCollection, isSameUserId);

            // ug.AllChildren.Any(g => directGroupIds.Contains(g.Id))
            var allChildrenCollection = LinqExpr.PropertyOrField(lambdaParamUsersGroup, "AllChildren");
            var isInChildGroup = LinqExpr.Call(groupAnyFunc, allChildrenCollection, isInGroupIds);

            // ((ICollection<TUser>)ug.Users).Any(u => u.Id == user.SecurityInfo.Identifier)
            // || ug.AllChildren.Any(g => directGroupIds.Contains(g.Id))
            var cond = LinqExpr.Lambda<Func<UsersGroup, bool>>(LinqExpr.OrElse(isUserInGroup, isInChildGroup), lambdaParamUsersGroup);

            var query = session.Query<UsersGroup>()
                .Where(cond);

            return query;
        }

        public static DetachedCriteria AllGroups<TEntity>(TEntity entity) where TEntity : class
        {
            Guid key = Security.ExtractKey(entity);
            DetachedCriteria directGroupsCriteria = DirectEntitiesGroups(entity)
                .SetProjection(Projections.Id());

            DetachedCriteria criteria = DetachedCriteria.For<EntitiesGroup>()
                                                        .CreateAlias("Entities", "entity", JoinType.LeftOuterJoin)
                                                        .CreateAlias("AllChildren", "child", JoinType.LeftOuterJoin)
                                                        .Add(
                                                            Subqueries.PropertyIn("child.id", directGroupsCriteria) ||
                                                            Expression.Eq("entity.EntitySecurityKey", key))
                                        .SetProjection(Projections.Id());
            
            return DetachedCriteria.For<EntitiesGroup>()
                                    .Add(Subqueries.PropertyIn("Id", criteria));
        }

        public static IQueryable<EntitiesGroup> AllGroups<TEntity>(TEntity entity, NHibernate.ISession session) where TEntity : class
        {
            Guid key = Security.ExtractKey(entity);
            var directGroupsIds = DirectEntitiesGroups(entity, session)
                .Select(x => x.Id);

            var query = session.Query<EntitiesGroup>()
                .Where(x =>
                    x.AllChildren.Any(c => directGroupsIds.Contains(c.Id))
                    || x.Entities.Any(e => e.EntitySecurityKey == key));

            return query;
        }
		
	}
}
