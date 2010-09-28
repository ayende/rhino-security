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

			// Delete account and confirm EntityReference was removed
			session.Delete(account);
			session.Flush();

			reference = session.CreateCriteria<EntityReference>()
				 .Add(Restrictions.Eq("EntitySecurityKey", securityKey))
				 .SetCacheable(true)
				 .UniqueResult<EntityReference>();

			Assert.Null(reference);
		}
	}
}
