namespace Rhino.Security.Tests
{
	using System;
	using NHibernate.Criterion;
	using Rhino.Security.Model;
	using Xunit;

	public class DeleteEntityEventListenerFixture : DatabaseFixture
	{
		[Fact]
		public void DoesDeletingEntityRemoveEntityReferences() {
			var account = new Account() { Name = "Bob" };
			session.Save(account);
			authorizationRepository.AssociateEntityWith(account, "Important Accounts");
			session.Flush();

			Guid securityKey = Security.ExtractKey(account);

			// Confirm EntityReference for the new account exists.
			EntityReference reference = session.CreateCriteria<EntityReference>()
				 .Add(Restrictions.Eq("EntitySecurityKey", securityKey))
				 .SetCacheable(true)
				 .UniqueResult<EntityReference>();

			Assert.NotNull(reference);

			// Create a permission for the entity
			this.permissionsBuilderService.Allow("/Account/Edit").For(this.user).On(account).DefaultLevel().Save();

			// Confirm the permissison for the entity exists
			Permission permission = session.CreateCriteria<Permission>()
				.Add(Restrictions.Eq("EntitySecurityKey", securityKey))
				.SetCacheable(true)
				.UniqueResult<Permission>();

			Assert.NotNull(permission);

			// Delete account 
			session.Delete(account);
			session.Flush();

			// Confirm EntityReference was removed
			reference = session.CreateCriteria<EntityReference>()
				 .Add(Restrictions.Eq("EntitySecurityKey", securityKey))
				 .SetCacheable(true)
				 .UniqueResult<EntityReference>();

			Assert.Null(reference);

			// Confirm the permissison for the entity was removed
			permission = session.CreateCriteria<Permission>()
				.Add(Restrictions.Eq("EntitySecurityKey", securityKey))
				.SetCacheable(true)
				.UniqueResult<Permission>();

			Assert.Null(permission);
		}
	}
}
