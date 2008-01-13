namespace Rhino.Security.Tests
{
    using System.Collections.Generic;
    using Commons;
    using MbUnit.Framework;

    [TestFixture]
    public class PermissionsServiceFixture : DatabaseFixture
    {
        private IPermissionsService permissionService;
        private IAuthorizationEditingService authorizationEditingService;
        private IPermissionsBuilderService permissionsBuilderService;
        private Account account;
        private User user;

        public override void SetUp()
        {
            base.SetUp();

            user = new User();
            this.user.Name = "Ayende";
            account = new Account();
            this.account.Name = "south sand";

            UnitOfWork.CurrentSession.Save(user);
            UnitOfWork.CurrentSession.Save(account);

            permissionService = IoC.Resolve<IPermissionsService>();
            permissionsBuilderService = IoC.Resolve<IPermissionsBuilderService>();
            authorizationEditingService = IoC.Resolve<IAuthorizationEditingService>();
            authorizationEditingService.CreateUsersGroup("Administrators");
            authorizationEditingService.CreateEntitiesGroup("Important Accounts");
            authorizationEditingService.CreateOperation("/Account/Edit");

            UnitOfWork.Current.TransactionalFlush();

            authorizationEditingService.AssociateUserWith(this.user, "Administrators");
            authorizationEditingService.AssociateEntityWith(this.account, "Important Accounts");

            UnitOfWork.Current.TransactionalFlush();
        }

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
                .Allow("/Account/Edit")
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
    }
}
