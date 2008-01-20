namespace Rhino.Security.Tests
{
	using System.Collections.Generic;
	using Commons;
	using MbUnit.Framework;

	[TestFixture]
	public class PermissionsServiceFixture : DatabaseFixture
	{
		[Test]
		public void CanCreatePermission()
		{
			Permission permission = permissionsBuilderService
				.Allow("/Account/Edit")
				.For(user)
				.On(account)
				.DefaultLevel()
				.Save();
			UnitOfWork.Current.TransactionalFlush();
			UnitOfWork.CurrentSession.Evict(permission);
			ICollection<Permission> all = Repository<Permission>.FindAll();

			Assert.AreEqual(1, all.Count);
		}

		[Test]
		public void CanGetPermissionByUser()
		{
			permissionsBuilderService
				.Allow("/Account/Edit")
				.For(user)
				.On(account)
				.DefaultLevel()
				.Save();
			UnitOfWork.Current.TransactionalFlush();

			Permission[] permissions = permissionService.GetPermissionsFor(user);
			Assert.AreEqual(1, permissions.Length);
		}

		[Test]
		public void CanGetPermissionsByUser_WhenDefinedOnGroup()
		{
			permissionsBuilderService
				.Allow("/Account/Edit")
				.For("Administrators")
				.On(account)
				.DefaultLevel()
				.Save();
			UnitOfWork.Current.TransactionalFlush();

			Permission[] permissions = permissionService.GetPermissionsFor(user);
			Assert.AreEqual(1, permissions.Length);
		}

		[Test]
		public void CanGetPermissionsByEntity()
		{
			permissionsBuilderService
				.Allow("/Account/Edit")
				.For("Administrators")
				.On(account)
				.DefaultLevel()
				.Save();
			UnitOfWork.Current.TransactionalFlush();

			Permission[] permissions = permissionService.GetPermissionsFor(account);
			Assert.AreEqual(1, permissions.Length);
		}

		[Test]
		public void CanGetPermissionsByEntity_WhenDefinedOnEntityGroup()
		{
			permissionsBuilderService
				.Allow("/Account/Edit")
				.For("Administrators")
				.On("Important Accounts")
				.DefaultLevel()
				.Save();
			UnitOfWork.Current.TransactionalFlush();

			Permission[] permissions = permissionService.GetPermissionsFor(account);
			Assert.AreEqual(1, permissions.Length);
		}

		[Test]
		public void CanGetPermissionsByUserAndEntity()
		{
			permissionsBuilderService
				.Deny("/Account/Edit")
				.For(user)
				.On(account)
				.DefaultLevel()
				.Save();
			UnitOfWork.Current.TransactionalFlush();

			Permission[] permissions = permissionService.GetPermissionsFor(user, account);
			Assert.AreEqual(1, permissions.Length);
		}

		[Test]
		public void CanGetPermissionsByUserAndOperationName()
		{
			permissionsBuilderService
				.Allow("/Account/Edit")
				.For(user)
				.On(account)
				.DefaultLevel()
				.Save();
			UnitOfWork.Current.TransactionalFlush();

			Permission[] permissions = permissionService.GetPermissionsFor(user, "/Account/Edit");
			Assert.AreEqual(1, permissions.Length);
		}

		[Test]
		public void CanGetPermissionsByUserAndOperationName_WhenParentOperationWasGranted()
		{
			permissionsBuilderService
				.Allow("/Account")
				.For(user)
				.On(account)
				.DefaultLevel()
				.Save();
			UnitOfWork.Current.TransactionalFlush();

			Permission[] permissions = permissionService.GetPermissionsFor(user, "/Account/Edit");
			Assert.AreEqual(1, permissions.Length);
		}

		[Test]
		public void CanGetPermissionByUserEntityAndOperation()
		{

			permissionsBuilderService
				.Allow("/Account")
				.For(user)
				.On("Important Accounts")
				.DefaultLevel()
				.Save();
			UnitOfWork.Current.TransactionalFlush();

			Permission[] permissions = permissionService.GetPermissionsFor(user, account, "/Account/Edit");
			Assert.AreEqual(1, permissions.Length);
		}

		[Test]
		public void PermissionsAreOrderedByLevelAndThenByDenyOrAllow()
		{
			permissionsBuilderService
			   .Allow("/Account")
			   .For(user)
			   .On("Important Accounts")
			   .DefaultLevel()
			   .Save();
			permissionsBuilderService
			   .Deny("/Account/Edit")
			   .For(user)
			   .On("Important Accounts")
			   .DefaultLevel()
			   .Save();
			permissionsBuilderService
			   .Allow("/Account/Edit")
			   .For(user)
			   .On("Important Accounts")
			   .Level(20)
			   .Save();
			permissionsBuilderService
				.Deny("/Account/Edit")
				.For(user)
				.On("Important Accounts")
				.Level(20)
				.Save();
			UnitOfWork.Current.TransactionalFlush();

			Permission[] permissions = permissionService.GetPermissionsFor(user);
			Assert.AreEqual(4, permissions.Length);

			Assert.AreEqual(20, permissions[0].Level);
			Assert.IsFalse(permissions[0].Allow);

			Assert.AreEqual(20, permissions[1].Level);
			Assert.IsTrue(permissions[1].Allow);

			Assert.AreEqual(1, permissions[2].Level);
			Assert.IsFalse(permissions[2].Allow);

			Assert.AreEqual(1, permissions[3].Level);
			Assert.IsTrue(permissions[3].Allow);
		}
	}
}
