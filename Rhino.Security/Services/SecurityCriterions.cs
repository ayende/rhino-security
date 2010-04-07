using System;

namespace Rhino.Security.Services
{
	using Model;
	using NHibernate.Criterion;
	using NHibernate.SqlCommand;

	/// <summary>
	/// A factory for common DetachedCriteria.
	/// </summary>
	internal class SecurityCriterions
	{
		public static DetachedCriteria DirectUsersGroups(IUser user)
		{
			return DetachedCriteria.For<UsersGroup>()
				.CreateAlias("Users", "user")
				.Add(Expression.Eq("user.id", user.SecurityInfo.Identifier));
		}

        public static DetachedCriteria DirectEntitiesGroups<TEntity>(TEntity entity) where TEntity : class
        {
            Guid key = Security.ExtractKey(entity);
            return DetachedCriteria.For<EntitiesGroup>()
                .CreateAlias("Entities", "e")
                .Add(Expression.Eq("e.EntitySecurityKey", key));
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
        
        public static DetachedCriteria AllGroups<TEntity>(TEntity entity)where TEntity:class
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
		
	}
}
