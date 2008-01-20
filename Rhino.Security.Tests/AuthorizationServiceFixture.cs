namespace Rhino.Security.Tests
{
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
			bool isAllowed = authorizationService.IsAllowed(user, "/Account/Edit");
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

	}
}