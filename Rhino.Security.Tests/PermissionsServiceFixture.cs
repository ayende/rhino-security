using Rhino.Security.Model;
using Xunit;

namespace Rhino.Security.Tests
{
    using System.Collections.Generic;

    public class PermissionsServiceFixture : DatabaseFixture
    {
        [Fact]
        public void CanCreatePermission()
        {
            Permission permission = permissionsBuilderService
                .Allow("/Account/Edit")
                .For(user)
                .On(account)
                .DefaultLevel()
                .Save();
            session.Flush();
            session.Evict(permission);
            ICollection<Permission> all = session.CreateCriteria<Permission>().List<Permission>();

            Assert.Equal(1, all.Count);
        }

        [Fact]
        public void CanGetPermissionByUser()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(user)
                .On(account)
                .DefaultLevel()
                .Save();
            session.Flush();

            Permission[] permissions = permissionService.GetPermissionsFor(user);
            Assert.Equal(1, permissions.Length);
        }

        [Fact]
        public void CanGetPermissionsByUser_WhenDefinedOnGroup()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For("Administrators")
                .On(account)
                .DefaultLevel()
                .Save();
            session.Flush();

            Permission[] permissions = permissionService.GetPermissionsFor(user);
            Assert.Equal(1, permissions.Length);
        }

        [Fact]
        public void CanGetPermissionsByEntity()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For("Administrators")
                .On(account)
                .DefaultLevel()
                .Save();
            session.Flush();

            Permission[] permissions = permissionService.GetPermissionsFor(account);
            Assert.Equal(1, permissions.Length);
        }

        [Fact]
        public void CanGetPermissionsByEntity_WhenDefinedOnEntityGroup()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For("Administrators")
                .On("Important Accounts")
                .DefaultLevel()
                .Save();
            session.Flush();

            Permission[] permissions = permissionService.GetPermissionsFor(account);
            Assert.Equal(1, permissions.Length);
        }

        [Fact]
        public void CanGetPermissionsByUserAndEntity()
        {
            permissionsBuilderService
                .Deny("/Account/Edit")
                .For(user)
                .On(account)
                .DefaultLevel()
                .Save();
            session.Flush();

            Permission[] permissions = permissionService.GetPermissionsFor(user, account);
            Assert.Equal(1, permissions.Length);
        }

        [Fact]
        public void CanGetPermissionsByUserAndOperationName()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(user)
                .On(account)
                .DefaultLevel()
                .Save();
            session.Flush();

            Permission[] permissions = permissionService.GetPermissionsFor(user, account);
            Assert.Equal(1, permissions.Length);
        }

        [Fact]
        public void CanGetPermissionsByUserAndOperationName_WhenParentOperationWasGranted()
        {
            permissionsBuilderService
                .Allow("/Account")
                .For(user)
                .On(account)
                .DefaultLevel()
                .Save();
            session.Flush();

            Permission[] permissions = permissionService.GetPermissionsFor(user, account);
            Assert.Equal(1, permissions.Length);
        }

        [Fact]
        public void CanGetPermissionsByUserAndOpernationName_WhenPermissionOnEverything()
        {
            permissionsBuilderService
                .Allow("/Account")
                .For(user)
                .OnEverything()
                .DefaultLevel()
                .Save();
            session.Flush(); 

            Permission[] permissions = permissionService.GetGlobalPermissionsFor(user, "/Account/Edit");
            Assert.Equal(1, permissions.Length);
        }        

        [Fact]
        public void CanGetPermissionByUserEntityAndOperation()
        {
            permissionsBuilderService
                .Allow("/Account")
                .For(user)
                .On("Important Accounts")
                .DefaultLevel()
                .Save();
            session.Flush();

            Permission[] permissions = permissionService.GetPermissionsFor(user, account, "/Account/Edit");
            Assert.Equal(1, permissions.Length);
        }

        [Fact]
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
            session.Flush();

            Permission[] permissions = permissionService.GetPermissionsFor(user);
            Assert.Equal(4, permissions.Length);

            Assert.Equal(20, permissions[0].Level);
            Assert.False(permissions[0].Allow);

            Assert.Equal(20, permissions[1].Level);
            Assert.True(permissions[1].Allow);

            Assert.Equal(1, permissions[2].Level);
            Assert.False(permissions[2].Allow);

            Assert.Equal(1, permissions[3].Level);
            Assert.True(permissions[3].Allow);
        }

        [Fact]
        public void CanSetPermissionOnEverythingAndGetItOnEntity()
        {
            permissionsBuilderService
                .Allow("/Account")
                .For(user)
                .OnEverything()
                .DefaultLevel()
                .Save();
            session.Flush();

            Permission[] permissions = permissionService.GetPermissionsFor(user, account, "/Account/Edit");
            Assert.Equal(1, permissions.Length);
        }

        [Fact]
        public void CanGetPermissionsSetOnarentGroupUserIsAssociatedWith()
        {
            authorizationRepository.CreateChildUserGroupOf("Administrators", "Helpdesk");
            

            authorizationRepository.DetachUserFromGroup(user, "Administrators");
            authorizationRepository.AssociateUserWith(user, "Helpdesk");
            

            permissionsBuilderService
                .Allow("/Account/Edit")
                .For("Administrators")
                .On("Important Accounts")
                .DefaultLevel()
                .Save();
            

            Permission[] permissions = permissionService.GetPermissionsFor(user, account, "/Account/Edit");
            Assert.Equal(1, permissions.Length);
        }

        [Fact]
        public void CanRemovePermission()
        {
            Permission permission = permissionsBuilderService
                .Allow("/Account/Edit")
                .For("Administrators")
                .On("Important Accounts")
                .DefaultLevel()
                .Save();
            session.Flush();

            Permission[] permissions = permissionService.GetPermissionsFor(user, account, "/Account/Edit");
            Assert.Equal(1, permissions.Length);

            authorizationRepository.RemovePermission(permission);
            

            permissions = permissionService.GetPermissionsFor(user, account, "/Account/Edit");
            Assert.Equal(0, permissions.Length);
        }
    }
}
