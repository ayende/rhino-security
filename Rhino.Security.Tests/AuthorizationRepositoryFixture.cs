using NHibernate.Exceptions;
using Rhino.Security.Model;
using Xunit;

namespace Rhino.Security.Tests
{
    using System;

    public class AuthorizationRepositoryFixture : DatabaseFixture
    {
        [Fact]
        public void CanSaveUser()
        {
            User ayende = new User {Name = "ayende"};
            session.Save(ayende);
            session.Flush();
            session.Evict(ayende);

            User fromDb = session.Get<User>(ayende.Id);
            Assert.NotNull(fromDb);
            Assert.Equal(ayende.Name, fromDb.Name);
        }

        [Fact]
        public void CanSaveAccount()
        {
            Account ayende = new Account {Name = "ayende"};
            Assert.NotEqual(Guid.Empty, ayende.SecurityKey);
            session.Save(ayende);
            session.Flush();
            session.Evict(ayende);

            Account fromDb = session.Get<Account>(ayende.Id);
            Assert.NotNull(fromDb);
            Assert.Equal(ayende.Name, fromDb.Name);
            Assert.Equal(fromDb.SecurityKey, ayende.SecurityKey);
        }

        [Fact]
        public void CanCreateUsersGroup()
        {
            UsersGroup group = authorizationRepository.CreateUsersGroup("Admininstrators");


            session.Flush();
            session.Evict(group);

            UsersGroup groupFromDb = session.Get<UsersGroup>(group.Id);
            Assert.NotNull(groupFromDb);
            Assert.Equal(group.Name, groupFromDb.Name);
        }

        [Fact]
        public void CanCreateEntitesGroup()
        {
            EntitiesGroup group = authorizationRepository.CreateEntitiesGroup("Accounts");


            session.Flush();
            session.Evict(group);

            EntitiesGroup groupFromDb = session.Get<EntitiesGroup>(group.Id);
            Assert.NotNull(groupFromDb);
            Assert.Equal(group.Name, groupFromDb.Name);
        }

        [Fact]
        public void CannotCreateEntitiesGroupWithSameName()
        {
            authorizationRepository.CreateEntitiesGroup("Admininstrators");
            session.Flush();

            Exception exception = Assert.Throws<GenericADOException>(() =>
                                                                         {
                                                                             authorizationRepository.CreateEntitiesGroup("Admininstrators");
                                                                             session.Flush();
                                                                         }).InnerException;
            Assert.Contains("unique", exception.Message);
        }

        [Fact]
        public void CannotCreateUsersGroupsWithSameName()
        {
            authorizationRepository.CreateUsersGroup("Admininstrators");
            session.Flush();

            Exception exception = Assert.Throws<GenericADOException>(() =>
                                                                         {
                                                                             authorizationRepository.CreateUsersGroup("Admininstrators");
                                                                             session.Flush();
                                                                         }).InnerException;

            Assert.Contains("unique", exception.Message);
        }

        [Fact]
        public void CanGetUsersGroupByName()
        {
            UsersGroup group = authorizationRepository.CreateUsersGroup("Admininstrators");

            session.Flush();
            session.Evict(group);

            group = authorizationRepository.GetUsersGroupByName("Admininstrators");
            Assert.NotNull(group);
        }

        [Fact]
        public void CanGetEntitiesGroupByName()
        {
            EntitiesGroup group = authorizationRepository.CreateEntitiesGroup("Accounts");


            session.Flush(); 
            session.Evict(group);

            group = authorizationRepository.GetEntitiesGroupByName("Accounts");
            Assert.NotNull(group);
        }

        [Fact]
        public void CanChangeUsersGroupName()
        {
            UsersGroup group = authorizationRepository.CreateUsersGroup("Admininstrators");

            session.Flush();
            session.Evict(group);

            authorizationRepository.RenameUsersGroup("Admininstrators", "2");


            session.Flush(); 
            session.Evict(group);

            group = authorizationRepository.GetUsersGroupByName("2");
            Assert.NotNull(group);
            group = authorizationRepository.GetUsersGroupByName("Admininstrators");
            Assert.Null(group);
        }

        [Fact]
        public void CannotRenameUsersGroupToAnAlreadyExistingUsersGroup()
        {
            UsersGroup group = authorizationRepository.CreateUsersGroup("Admininstrators");
            UsersGroup group2 = authorizationRepository.CreateUsersGroup("ExistingGroup");

            session.Flush();

            session.Evict(group);
            session.Evict(group2);

            Exception exception = Assert.Throws<GenericADOException>(
                () =>
                    {
                        authorizationRepository.RenameUsersGroup("Admininstrators", "ExistingGroup");
                        session.Flush();
                    }).InnerException;
            Assert.Contains("unique", exception.Message);
        }

        [Fact]
        public void CanChangeEntitiesGroupName()
        {
            EntitiesGroup group = authorizationRepository.CreateEntitiesGroup("Accounts");

            session.Flush();
            session.Evict(group);

            authorizationRepository.RenameEntitiesGroup("Accounts", "2");


            session.Evict(group);

            group = authorizationRepository.GetEntitiesGroupByName("2");
            Assert.NotNull(group);
            group = authorizationRepository.GetEntitiesGroupByName("Accounts");
            Assert.Null(group);
        }

        [Fact]
        public void CannotRenameEntitiesGroupToAnAlreadyExistingEntitiesGroup()
        {
            EntitiesGroup group = authorizationRepository.CreateEntitiesGroup("Accounts");
            EntitiesGroup group2 = authorizationRepository.CreateEntitiesGroup("ExistingGroup");


            session.Flush();
            session.Evict(group);
            session.Evict(group2);

            Exception exception = Assert.Throws<GenericADOException>(
                () =>
                    {
                        authorizationRepository.RenameEntitiesGroup("Accounts", "ExistingGroup");
                        session.Flush();
                    }).InnerException;

            Assert.Contains("unique", exception.Message);
        }

        [Fact]
        public void CannotRenameUsersGroupThatDoesNotExist()
        {
            Assert.Throws<InvalidOperationException>("There is no users group named: NonExistingGroup",
                                                     () =>
                                                     authorizationRepository.RenameUsersGroup("NonExistingGroup",
                                                                                              "Administrators"));
        }

        [Fact]
        public void CannotRenameEntitiesGroupThatDoesNotExist()
        {
            Assert.Throws<InvalidOperationException>( "There is no entities group named: NonExistingGroup",
                                                     () =>
                                                     authorizationRepository.RenameEntitiesGroup("NonExistingGroup",
                                                                                                 "Accounts"));
        }


        [Fact]
        public void CanAssociateUserWithGroup()
        {
            User ayende = new User {Name = "ayende"};

            session.Save(ayende);
            UsersGroup group = authorizationRepository.CreateUsersGroup("Admins");


            authorizationRepository.AssociateUserWith(ayende, "Admins");

            session.Flush();
            session.Evict(ayende);
            session.Evict(group);

            UsersGroup[] groups = authorizationRepository.GetAssociatedUsersGroupFor(ayende);
            Assert.Equal(1, groups.Length);
            Assert.Equal("Admins", groups[0].Name);
        }

        [Fact]
        public void CanAssociateAccountWithMultipleGroups()
        {
            Account ayende = new Account();
            ayende.Name = "ayende";

            session.Save(ayende);
            EntitiesGroup group = authorizationRepository.CreateEntitiesGroup("Accounts");
            EntitiesGroup group2 = authorizationRepository.CreateEntitiesGroup("Accounts of second group");


            authorizationRepository.AssociateEntityWith(ayende, "Accounts");

            authorizationRepository.AssociateEntityWith(ayende, "Accounts of second group");

            session.Flush();

            session.Evict(ayende);
            session.Evict(group);
            session.Evict(group2);

            EntitiesGroup[] groups = authorizationRepository.GetAssociatedEntitiesGroupsFor(ayende);
            Assert.Equal(2, groups.Length);
            Assert.Equal("Accounts", groups[0].Name);
            Assert.Equal("Accounts of second group", groups[1].Name);
        }

        [Fact]
        public void CanAssociateUserWithNestedGroup()
        {
            User ayende = new User();
            ayende.Name = "ayende";

            session.Save(ayende);
            authorizationRepository.CreateUsersGroup("Admins");

            UsersGroup group = authorizationRepository.CreateChildUserGroupOf("Admins", "DBA");


            authorizationRepository.AssociateUserWith(ayende, "DBA");

            session.Flush();
            session.Evict(ayende);
            session.Evict(group);

            UsersGroup[] groups = authorizationRepository.GetAssociatedUsersGroupFor(ayende);
            Assert.Equal(2, groups.Length);
            Assert.Equal("Admins", groups[0].Name);
            Assert.Equal("DBA", groups[1].Name);
        }

        [Fact]
        public void CanAssociateAccountWithNestedGroup()
        {
            Account beto = new Account();
            beto.Name = "beto account";

            session.Save(beto);
            authorizationRepository.CreateEntitiesGroup("Executive Accounts");

            EntitiesGroup group = authorizationRepository.CreateChildEntityGroupOf("Executive Accounts", "Manager Accounts");

            authorizationRepository.AssociateEntityWith(beto,"Manager Accounts");
            
            session.Flush();
            session.Evict(beto);
            session.Evict(group);

            EntitiesGroup[] groups = authorizationRepository.GetAssociatedEntitiesGroupsFor(beto);
            Assert.Equal(2, groups.Length);
            Assert.Equal("Executive Accounts",groups[0].Name);
            Assert.Equal("Manager Accounts", groups[1].Name);
        }


        [Fact]
        public void CanGetAncestryAssociationOfUserWithGroupWithNested()
        {
            User ayende = new User();
            ayende.Name = "ayende";

            session.Save(ayende);
            authorizationRepository.CreateUsersGroup("Admins");

            authorizationRepository.CreateChildUserGroupOf("Admins", "DBA");


            authorizationRepository.AssociateUserWith(ayende, "DBA");


            UsersGroup[] groups = authorizationRepository.GetAncestryAssociation(ayende, "Admins");
            Assert.Equal(2, groups.Length);
            Assert.Equal("DBA", groups[0].Name);
            Assert.Equal("Admins", groups[1].Name);
        }

        [Fact]
        public void CanGetAncestryAssociationOfAccountWithGroupWithNested()
        {
            Account beto = new Account();
            beto.Name = "beto account";

            session.Save(beto);
            authorizationRepository.CreateEntitiesGroup("Executive Accounts");

            authorizationRepository.CreateChildEntityGroupOf("Executive Accounts", "Manager Accounts");

            authorizationRepository.AssociateEntityWith(beto, "Manager Accounts");

            EntitiesGroup[] groups = authorizationRepository.GetAncestryAssociationOfEntity(beto,
                                                                                            "Executive Accounts");
            Assert.Equal(2,groups.Length);
            Assert.Equal("Manager Accounts", groups[0].Name);
            Assert.Equal("Executive Accounts", groups[1].Name);
        }

        [Fact]
        public void CanGetAncestryAssociationOfUserWithGroupDirect()
        {
            User ayende = new User();
            ayende.Name = "ayende";

            session.Save(ayende);
            authorizationRepository.CreateUsersGroup("Admins");


            authorizationRepository.AssociateUserWith(ayende, "Admins");


            UsersGroup[] groups = authorizationRepository.GetAncestryAssociation(ayende, "Admins");
            Assert.Equal(1, groups.Length);
            Assert.Equal("Admins", groups[0].Name);
        }

        [Fact]
        public void CanGetAncestryAssociationOfAccountWithGroupDirect()
        {
            Account beto = new Account();
            beto.Name = "beto account";

            session.Save(beto);
            authorizationRepository.CreateEntitiesGroup("Executive Accounts");

            authorizationRepository.AssociateEntityWith(beto,"Executive Accounts");

            EntitiesGroup[] groups = authorizationRepository.GetAncestryAssociationOfEntity(beto, "Executive Accounts");
            Assert.Equal(1, groups.Length);
            Assert.Equal("Executive Accounts", groups[0].Name);
        }

        [Fact]
        public void CanGetAncestryAssociationOfUserWithGroupWhereNonExists()
        {
            User ayende = new User();
            ayende.Name = "ayende";

            session.Save(ayende);
            authorizationRepository.CreateUsersGroup("Admins");



            UsersGroup[] groups = authorizationRepository.GetAncestryAssociation(ayende, "Admins");
            Assert.Equal(0, groups.Length);
        }
        
        [Fact]
        public void CanGetAncestryAssociationOfEntityWithGroupWhereNonExists()
        {
            Account beto = new Account();
            beto.Name = "beto account";

            session.Save(beto);
            authorizationRepository.CreateEntitiesGroup("Executive Accounts");

            EntitiesGroup[] groups = authorizationRepository.GetAncestryAssociationOfEntity(beto, "Executive Accounts");
            Assert.Equal(0,groups.Length);
        }

        [Fact]
        public void CanGetAncestryAssociationOfUserWithGroupWhereThereIsDirectPathShouldSelectThat()
        {
            User ayende = new User();
            ayende.Name = "ayende";

            session.Save(ayende);
            authorizationRepository.CreateUsersGroup("Admins");


            authorizationRepository.CreateChildUserGroupOf("Admins", "DBA");

            authorizationRepository.AssociateUserWith(ayende, "Admins");
            authorizationRepository.AssociateUserWith(ayende, "DBA");


            UsersGroup[] groups = authorizationRepository.GetAncestryAssociation(ayende, "Admins");
            Assert.Equal(1, groups.Length);
            Assert.Equal("Admins", groups[0].Name);
        }

        [Fact]
        public void CanGetAncestryAssociationOfAccountWithGroupWhereThereIsDirectPathShouldSelectThat()
        {
            Account beto = new Account();
            beto.Name = "beto account";

            session.Save(beto);
            authorizationRepository.CreateEntitiesGroup("Executive Accounts");

            authorizationRepository.CreateChildEntityGroupOf("Executive Accounts", "Manager Accounts");
            
            authorizationRepository.AssociateEntityWith(beto,"Executive Accounts");
            authorizationRepository.AssociateEntityWith(beto, "Manager Accounts");

            EntitiesGroup[] groups = authorizationRepository.GetAncestryAssociationOfEntity(beto, "Executive Accounts");
            Assert.Equal(1, groups.Length);
            Assert.Equal("Executive Accounts",groups[0].Name);
        }

        [Fact]
        public void CanGetAncestryAssociationOfUserWithGroupWhereThereIsTwoLevelNesting()
        {
            User ayende = new User();
            ayende.Name = "ayende";

            session.Save(ayende);
            authorizationRepository.CreateUsersGroup("Admins");


            authorizationRepository.CreateChildUserGroupOf("Admins", "DBA");

            authorizationRepository.CreateChildUserGroupOf("DBA", "SQLite DBA");

            authorizationRepository.AssociateUserWith(ayende, "SQLite DBA");


            UsersGroup[] groups = authorizationRepository.GetAncestryAssociation(ayende, "Admins");
            Assert.Equal(3, groups.Length);
            Assert.Equal("SQLite DBA", groups[0].Name);
            Assert.Equal("DBA", groups[1].Name);
            Assert.Equal("Admins", groups[2].Name);
        }

        [Fact]
        public void CanGetAncestryAssociationOfAccountWithGroupWhereThereIsTwoLevelNesting()
        {
            Account beto = new Account();
            beto.Name = "beto account";

            session.Save(beto);
            authorizationRepository.CreateEntitiesGroup("Executive Accounts");

            authorizationRepository.CreateChildEntityGroupOf("Executive Accounts", "Manager Accounts");

            authorizationRepository.CreateChildEntityGroupOf("Manager Accounts", "Employee Accounts");

            authorizationRepository.AssociateEntityWith(beto, "Employee Accounts");

            EntitiesGroup[] groups = authorizationRepository.GetAncestryAssociationOfEntity(beto, "Executive Accounts");
            Assert.Equal(3, groups.Length);
            Assert.Equal("Employee Accounts", groups[0].Name);
            Assert.Equal("Manager Accounts", groups[1].Name);
            Assert.Equal("Executive Accounts", groups[2].Name);
        }

        [Fact]
        public void CanGetAncestryAssociationOfUserWithGroupWhereThereIsMoreThanOneIndirectPathShouldSelectShortest()
        {
            User ayende = new User();
            ayende.Name = "ayende";

            session.Save(ayende);
            authorizationRepository.CreateUsersGroup("Admins");


            authorizationRepository.CreateChildUserGroupOf("Admins", "DBA");

            authorizationRepository.CreateChildUserGroupOf("DBA", "SQLite DBA");

            authorizationRepository.AssociateUserWith(ayende, "DBA");
            authorizationRepository.AssociateUserWith(ayende, "SQLite DBA");


            UsersGroup[] groups = authorizationRepository.GetAncestryAssociation(ayende, "Admins");
            Assert.Equal(2, groups.Length);
            Assert.Equal("DBA", groups[0].Name);
            Assert.Equal("Admins", groups[1].Name);
        }

        [Fact]
        public void CanGetAncestryAssociationOfAccountWithGroupWhereThereIsMoreThanOneIndirectPathShouldSelectShortest()
        {
            Account beto = new Account();
            beto.Name = "beto account";

            session.Save(beto);
            authorizationRepository.CreateEntitiesGroup("Executive Accounts");

            authorizationRepository.CreateChildEntityGroupOf("Executive Accounts", "Manager Accounts");

            authorizationRepository.CreateChildEntityGroupOf("Manager Accounts", "Employee Accounts");

            authorizationRepository.AssociateEntityWith(account, "Manager Accounts");
            authorizationRepository.AssociateEntityWith(account, "Employee Accounts");

            EntitiesGroup[] groups = authorizationRepository.GetAncestryAssociationOfEntity(account,
                                                                                            "Executive Accounts");
            Assert.Equal(2, groups.Length);
            Assert.Equal("Manager Accounts", groups[0].Name);
            Assert.Equal("Executive Accounts", groups[1].Name);
        }

        [Fact]
        public void CanAssociateAccountWithGroup()
        {
            Account ayende = new Account();
            ayende.Name = "ayende";

            session.Save(ayende);
            EntitiesGroup group = authorizationRepository.CreateEntitiesGroup("Accounts");


            authorizationRepository.AssociateEntityWith(ayende, "Accounts");


            session.Flush();
            session.Evict(ayende);
            session.Evict(group);

            EntitiesGroup[] groups = authorizationRepository.GetAssociatedEntitiesGroupsFor(ayende);
            Assert.Equal(1, groups.Length);
            Assert.Equal("Accounts", groups[0].Name);
        }

        [Fact]
        public void CanCreateOperation()
        {
            authorizationRepository.CreateOperation("/Account/Delete");

            Operation operation = authorizationRepository.GetOperationByName("/Account/Delete");
            Assert.NotNull(operation);
        }

        [Fact]
        public void WhenCreatingNestedOperation_WillCreateParentOperation_IfDoesNotExists()
        {
            Operation operation = authorizationRepository.CreateOperation("/Account/Delete");

            Operation parentOperation = authorizationRepository.GetOperationByName("/Account");
            Assert.NotNull(parentOperation);
            Assert.Equal(operation.Parent, parentOperation);
        }

        [Fact]
        public void WhenCreatingNestedOperation_WillLinkToParentOperation()
        {
            authorizationRepository.CreateOperation("/Account/Delete");

            Operation parentOperation = authorizationRepository.GetOperationByName("/Account");
            Assert.NotNull(parentOperation); // was created in setup
            Assert.Equal(2, parentOperation.Children.Count); // /Edit, /Delete
        }

        [Fact]
        public void CanRemoveUserGroup()
        {
            authorizationRepository.RemoveUsersGroup("Administrators");


            Assert.Null(authorizationRepository.GetUsersGroupByName("Administrators"));
        }

        [Fact]
        public void RemovingParentUserGroupWillFail()
        {
            authorizationRepository.CreateChildUserGroupOf("Administrators", "DBA");

            Assert.Throws<InvalidOperationException>(
                 "Cannot remove users group 'Administrators' because is has child groups. Remove those groups and try again.",
                 () => authorizationRepository.RemoveUsersGroup("Administrators"));
        }

        [Fact]
        public void RemovingParentEntityGroupWillFail()
        {
            authorizationRepository.CreateChildEntityGroupOf("Important Accounts", "Regular Accounts");

            Assert.Throws<InvalidOperationException>(
                 "Cannot remove entity group 'Important Accounts' because is has child groups. Remove those groups and try again.",
                 () => authorizationRepository.RemoveEntitiesGroup("Important Accounts"));
        }


        [Fact]
        public void WhenRemovingUsersGroupThatHasAssociatedPermissionsThoseShouldBeRemoved()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For("Administrators")
                .OnEverything()
                .DefaultLevel()
                .Save();


            Permission[] permissions = permissionService.GetPermissionsFor(user);
            Assert.NotEmpty(permissions);

            authorizationRepository.RemoveUsersGroup("Administrators");


            permissions = permissionService.GetPermissionsFor(user);
            Assert.Empty(permissions);
        }

        [Fact]
        public void CanRemoveNestedUserGroup()
        {
            UsersGroup dbaGroup = authorizationRepository.CreateChildUserGroupOf("Administrators", "DBA");


            authorizationRepository.RemoveUsersGroup("DBA");


            Assert.Null(authorizationRepository.GetUsersGroupByName("DBA"));

            UsersGroup administratorsGroup =
                authorizationRepository.GetUsersGroupByName("Administrators");
            Assert.Equal(0,
                            administratorsGroup.DirectChildren.Count
                );
            Assert.Equal(0,
                            administratorsGroup.AllChildren.Count
                );

            Assert.Equal(0, dbaGroup.AllParents.Count);
        }

        [Fact]
        public void CanRemoveNestedEntityGroup()
        {
            EntitiesGroup regularAccounts = authorizationRepository.CreateChildEntityGroupOf("Important Accounts",
                                                                                            "Regular Accounts");
            authorizationRepository.RemoveEntitiesGroup("Regular Accounts");

            Assert.Null(authorizationRepository.GetEntitiesGroupByName("Regular Accounts"));

            EntitiesGroup importantAccounts = authorizationRepository.GetEntitiesGroupByName("Important Accounts");

            Assert.Equal(0, importantAccounts.DirectChildren.Count);
            Assert.Equal(0, importantAccounts.AllChildren.Count);
            Assert.Equal(0, regularAccounts.AllParents.Count);
        }

        [Fact]
        public void UsersAreNotAssociatedWithRemovedGroups()
        {
            authorizationRepository.CreateChildUserGroupOf("Administrators", "DBA");


            authorizationRepository.AssociateUserWith(user, "DBA");

            session.Flush();

            UsersGroup[] associedGroups = authorizationRepository.GetAssociatedUsersGroupFor(user);
            Assert.Equal(2, associedGroups.Length);

            authorizationRepository.RemoveUsersGroup("DBA");

            session.Flush();

            associedGroups = authorizationRepository.GetAssociatedUsersGroupFor(user);
            Assert.Equal(1, associedGroups.Length);
        }

        [Fact]
        public void AccountsAreNotAssociatedWithRemovedGroups()
        {
            authorizationRepository.CreateChildEntityGroupOf("Important Accounts", "Regular Accounts");
            
            authorizationRepository.AssociateEntityWith(account,"Regular Accounts");
            
            session.Flush();

            EntitiesGroup[] associatedGroups = authorizationRepository.GetAssociatedEntitiesGroupsFor(account);
            Assert.Equal(2, associatedGroups.Length);
            
            authorizationRepository.RemoveEntitiesGroup("Regular Accounts");
            session.Flush();

            associatedGroups = authorizationRepository.GetAssociatedEntitiesGroupsFor(account);
            Assert.Equal(1, associatedGroups.Length);
        }

        [Fact]
        public void CanRemoveEntitiesGroup()
        {
            authorizationRepository.RemoveEntitiesGroup("Important Accounts");

            Assert.Null(authorizationRepository.GetEntitiesGroupByName("Important Accounts")); ;
        }


        [Fact]
        public void WhenRemovingEntitiesGroupAllPermissionsOnItWillBeDeleted()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(user)
                .On("Important Accounts")
                .DefaultLevel()
                .Save();


            Permission[] permissions = permissionService.GetPermissionsFor(user);
            Assert.NotEmpty(permissions);

            authorizationRepository.RemoveEntitiesGroup("Important Accounts");


            permissions = permissionService.GetPermissionsFor(user);
            Assert.Empty(permissions);
        }

        [Fact]
        public void CanRemoveOperation()
        {
            authorizationRepository.RemoveOperation("/Account/Edit");

            Assert.Null(authorizationRepository.GetOperationByName("/Account/Edit"));
        }

        [Fact]
        public void CannotRemoveParentOperatio()
        {
            Assert.Throws<InvalidOperationException>("Cannot remove operation '/Account' because it has child operations. Remove those operations and try again.",
                () => authorizationRepository.RemoveOperation("/Account"));
        }

        [Fact]
        public void CanRemoveNestedOperation()
        {
            authorizationRepository.RemoveOperation("/Account/Edit");

            Operation parent = authorizationRepository.GetOperationByName("/Account");

            Assert.Equal(0, parent.Children.Count);
        }

        [Fact]
        public void CanRemoveUser()
        {
            authorizationRepository.RemoveUser(user);
            session.Delete(user);

        }

        [Fact]
        public void RemovingUserWillAlsoRemoveAssociatedPermissions()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(user)
                .OnEverything()
                .DefaultLevel()
                .Save();

            authorizationRepository.RemoveUser(user);
            session.Delete(user);

        }
    }
}