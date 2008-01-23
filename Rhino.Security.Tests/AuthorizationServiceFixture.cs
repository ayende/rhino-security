namespace Rhino.Security.Tests
{
	using System.Data;
	using Commons;
	using MbUnit.Framework;

	[TestFixture]
	public class AuthorizationServiceFixture : DatabaseFixture
	{
		[Test]
		public void WillReturnFalseIfNoPermissionHasBeenDefined()
		{
			bool isAllowed = authorizationService.IsAllowed(user, "/Account/Edit");
			Assert.IsFalse(isAllowed);
		}

		[Test]
		public void WillReturnFalseIfOperationNotDefined()
		{
			bool isAllowed = authorizationService.IsAllowed(user, "/Account/Delete");
			Assert.IsFalse(isAllowed);
		}

		[Test]
		public void WillReturnTrueIfAllowPermissionWasDefined()
		{
			permissionsBuilderService
				.Allow("/Account/Edit")
				.For(user)
				.OnEverything()
				.DefaultLevel()
				.Save();
			UnitOfWork.Current.TransactionalFlush();

			bool isAllowed = authorizationService.IsAllowed(user, "/Account/Edit");
			Assert.IsTrue(isAllowed);
		}

		[Test]
		public void WillReturnFalseIfAllowPermissionWasDefinedOnGroupAndDenyPermissionOnUser()
		{
			permissionsBuilderService
				.Allow("/Account/Edit")
				.For(user)
				.OnEverything()
				.DefaultLevel()
				.Save();
			permissionsBuilderService
				.Deny("/Account/Edit")
				.For("Administrators")
				.OnEverything()
				.DefaultLevel()
				.Save();
			UnitOfWork.Current.TransactionalFlush();

			bool isAllowed = authorizationService.IsAllowed(user, "/Account/Edit");
			Assert.IsFalse(isAllowed);
		}

		[Test]
		public void WillReturnFalseIfAllowedPermissionWasDefinedWithDenyPermissionWithHigherLevel()
		{
			permissionsBuilderService
				.Allow("/Account/Edit")
				.For(user)
				.OnEverything()
				.DefaultLevel()
				.Save();
			permissionsBuilderService
				.Deny("/Account/Edit")
				.For("Administrators")
				.OnEverything()
				.Level(5)
				.Save();
			UnitOfWork.Current.TransactionalFlush();

			bool isAllowed = authorizationService.IsAllowed(user, "/Account/Edit");
			Assert.IsFalse(isAllowed);
		}

		[Test]
		public void WillReturnTrueIfAllowedPermissionWasDefinedWithDenyPermissionWithLowerLevel()
		{
			permissionsBuilderService
				.Allow("/Account/Edit")
				.For(user)
				.OnEverything()
				.Level(10)
				.Save();
			permissionsBuilderService
				.Deny("/Account/Edit")
				.For("Administrators")
				.OnEverything()
				.Level(5)
				.Save();
			UnitOfWork.Current.TransactionalFlush();

			bool isAllowed = authorizationService.IsAllowed(user, "/Account/Edit");
			Assert.IsTrue(isAllowed);
		}


		[Test]
		public void WillReturnTrueOnAccountIfPermissionWasGrantedOnAnything()
		{
			permissionsBuilderService
				.Allow("/Account/Edit")
				.For(user)
				.OnEverything()
				.DefaultLevel()
				.Save();
			UnitOfWork.Current.TransactionalFlush();

			bool isAllowed = authorizationService.IsAllowed(user, account, "/Account/Edit");
			Assert.IsTrue(isAllowed);
		}

		[Test]
		public void WillReturnFalseOnAccountIfPermissionWasDeniedOnAnything()
		{
			permissionsBuilderService
				.Deny("/Account/Edit")
				.For(user)
				.OnEverything()
				.DefaultLevel()
				.Save();
			UnitOfWork.Current.TransactionalFlush();

			bool isAllowed = authorizationService.IsAllowed(user, account, "/Account/Edit");
			Assert.IsFalse(isAllowed);
		}

		[Test]
		public void WillReturnTrueOnAccountIfPermissionWasGrantedOnGroupAssociatedWithUser()
		{
			permissionsBuilderService
				.Allow("/Account/Edit")
				.For("Administrators")
				.On(account)
				.DefaultLevel()
				.Save();
			UnitOfWork.Current.TransactionalFlush();

			bool isAllowed = authorizationService.IsAllowed(user, account, "/Account/Edit");
			Assert.IsTrue(isAllowed);
		}

		[Test]
		public void WillReturnFalseOnAccountIfPermissionWasDeniedOnGroupAssociatedWithUser()
		{
			permissionsBuilderService
				.Deny("/Account/Edit")
				.For("Administrators")
				.On(account)
				.DefaultLevel()
				.Save();
			UnitOfWork.Current.TransactionalFlush();

			bool isAllowed = authorizationService.IsAllowed(user, account, "/Account/Edit");
			Assert.IsFalse(isAllowed);
		}

		[Test]
		public void WillReturnTrueOnAccountIfPermissionWasGrantedToUser()
		{
			permissionsBuilderService
				.Allow("/Account/Edit")
				.For(user)
				.On(account)
				.DefaultLevel()
				.Save();
			UnitOfWork.Current.TransactionalFlush();

			bool isAllowed = authorizationService.IsAllowed(user, account, "/Account/Edit");
			Assert.IsTrue(isAllowed);
		}

		[Test]
		public void WillReturnFalseOnAccountIfPermissionWasDeniedToUser()
		{
			permissionsBuilderService
				.Deny("/Account/Edit")
				.For(user)
				.On(account)
				.DefaultLevel()
				.Save();
			UnitOfWork.Current.TransactionalFlush();

			bool isAllowed = authorizationService.IsAllowed(user, account, "/Account/Edit");
			Assert.IsFalse(isAllowed);
		}

		[Test]
		public void WillReturnTrueOnEntityGroupIfPermissionWasGrantedToUsersGroupAssociatedWithUser()
		{
			permissionsBuilderService
				.Allow("/Account/Edit")
				.For("Administrators")
				.On("Important Accounts")
				.DefaultLevel()
				.Save();
			UnitOfWork.Current.TransactionalFlush();

			bool isAllowed = authorizationService.IsAllowed(user, account, "/Account/Edit");
			Assert.IsTrue(isAllowed);
		}

		[Test]
		public void WillReturnFalseOnAccountIfNoPermissionIsDefined()
		{
			bool isAllowed = authorizationService.IsAllowed(user, account, "/Account/Edit");
			Assert.IsFalse(isAllowed);
		}

		[Test]
		public void WillReturnFalseOnAccountIfPermissionWasDeniedToUserOnTheGroupTheEntityIsAssociatedWith()
		{
			permissionsBuilderService
				.Deny("/Account/Edit")
				.For(user)
				.On("Important Accounts")
				.DefaultLevel()
				.Save();
			UnitOfWork.Current.TransactionalFlush();

			bool isAllowed = authorizationService.IsAllowed(user, account, "/Account/Edit");
			Assert.IsFalse(isAllowed);
		}

		[Test]
		public void WillReturnTrueOnAccountIfPermissionWasAllowedToUserOnTheGroupTheEntityIsAssociatedWith()
		{
			permissionsBuilderService
				.Allow("/Account/Edit")
				.For(user)
				.On("Important Accounts")
				.DefaultLevel()
				.Save();
			UnitOfWork.Current.TransactionalFlush();

			bool isAllowed = authorizationService.IsAllowed(user, account, "/Account/Edit");
			Assert.IsTrue(isAllowed);
		}

		[Test]
		public void UseSecondLevelCacheForSecurityQuestions()
		{
			permissionsBuilderService
				.Allow("/Account/Edit")
				.For(user)
				.On("Important Accounts")
				.DefaultLevel()
				.Save();
			UnitOfWork.Current.TransactionalFlush();

			bool isAllowed = authorizationService.IsAllowed(user, account, "/Account/Edit");
			Assert.IsTrue(isAllowed);

			// remove from DB without going through NHibernate
			using (IDbCommand command = UnitOfWork.CurrentSession.Connection.CreateCommand())
			{
				command.CommandText = "DELETE FROM security_Permissions";
				command.ExecuteNonQuery();
			}
			// using a new connection, so doesn't have access to it
			using (UnitOfWork.Start(UnitOfWorkNestingOptions.CreateNewOrNestUnitOfWork))
			{
				// should return true since it loads from cache
				isAllowed = authorizationService.IsAllowed(user, account, "/Account/Edit");
				Assert.IsTrue(isAllowed);
			}
		}


		[Test]
		public void UseSecondLevelCacheForSecurityQuestions_WillBeUpdatedWhenGoingThroughNHiberante()
		{
			permissionsBuilderService
				.Allow("/Account/Edit")
				.For(user)
				.On("Important Accounts")
				.DefaultLevel()
				.Save();
			UnitOfWork.Current.TransactionalFlush();

			bool isAllowed = authorizationService.IsAllowed(user, account, "/Account/Edit");
			Assert.IsTrue(isAllowed);

			Repository<Permission>.DeleteAll();
			UnitOfWork.Current.TransactionalFlush();

			// should return true since it loads from cache
			isAllowed = authorizationService.IsAllowed(user, account, "/Account/Edit");
			Assert.IsFalse(isAllowed);
		}


		[Test]
		public void WillReturnFalseIfPermissionWasAllowedToChildGroupUserIsAssociatedWith()
		{
			authorizationEditingService.CreateChildUserGroupOf("Administrators", "Helpdesk");
			UnitOfWork.Current.TransactionalFlush();

			permissionsBuilderService
			   .Allow("/Account/Edit")
			   .For("Helpdesk")
			   .On("Important Accounts")
			   .DefaultLevel()
			   .Save();
			UnitOfWork.Current.TransactionalFlush();

			bool isAllowed = authorizationService.IsAllowed(user, account, "/Account/Edit");
			Assert.IsFalse(isAllowed);
		}

		[Test]
		public void WillReturnTrueIfPermissionWasAllowedToParentGroupUserIsAssociatedWith()
		{
			authorizationEditingService.CreateChildUserGroupOf("Administrators", "Helpdesk");
			UnitOfWork.Current.TransactionalFlush();

			authorizationEditingService.DetachUserFromGroup(user, "Administrators");
			authorizationEditingService.AssociateUserWith(user, "Helpdesk");
			UnitOfWork.Current.TransactionalFlush();

			permissionsBuilderService
			   .Allow("/Account/Edit")
			   .For("Administrators")
			   .On("Important Accounts")
			   .DefaultLevel()
			   .Save();
			UnitOfWork.Current.TransactionalFlush();

			bool isAllowed = authorizationService.IsAllowed(user, account, "/Account/Edit");
			Assert.IsTrue(isAllowed);
		}
	}
}