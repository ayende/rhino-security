using Rhino.Commons.ForTesting;
using Rhino.Security.Exceptions;
using Rhino.Security.Model;

namespace Rhino.Security.Tests
{
    using System;
    using Commons;
    using MbUnit.Framework;

	[TestFixture]
	public class ActiveRecord_AuthorizationRepositoryFixture : AuthorizationRepositoryFixture
	{
		public override string RhinoContainerConfig
		{
			get { return "ar-windsor.boo"; }
		}

		public override PersistenceFramework PersistenceFramwork
		{
			get { return PersistenceFramework.ActiveRecord; }
		}
	}

	[TestFixture]
	public class NHibernate_AuthorizationRepositoryFixture : AuthorizationRepositoryFixture
	{
		public override string RhinoContainerConfig
		{
			get { return "nh-windsor.boo"; }
		}

		public override PersistenceFramework PersistenceFramwork
		{
			get { return PersistenceFramework.NHibernate; }
		}
	}

    public abstract class AuthorizationRepositoryFixture : DatabaseFixture
    {
        [Test]
        public void CanSaveUser()
        {
            User ayende = new User();
            ayende.Name = "ayende";
            UnitOfWork.CurrentSession.Save(ayende);
            UnitOfWork.CurrentSession.Flush();
            UnitOfWork.CurrentSession.Evict(ayende);

            User fromDb = UnitOfWork.CurrentSession.Get<User>(ayende.Id);
            Assert.IsNotNull(fromDb);
            Assert.AreEqual(ayende.Name, fromDb.Name);
        }

        [Test]
        public void CanSaveAccount()
        {
            Account ayende = new Account();
            ayende.Name = "ayende";
            Assert.AreNotEqual(Guid.Empty, ayende.SecurityKey);
            UnitOfWork.CurrentSession.Save(ayende);
            UnitOfWork.CurrentSession.Flush();
            UnitOfWork.CurrentSession.Evict(ayende);

            Account fromDb = UnitOfWork.CurrentSession.Get<Account>(ayende.Id);
            Assert.IsNotNull(fromDb);
            Assert.AreEqual(ayende.Name, fromDb.Name);
            Assert.AreEqual(fromDb.SecurityKey, ayende.SecurityKey);
        }

        [Test]
        public void CanCreateUsersGroup()
        {
            UsersGroup group = authorizationRepository.CreateUsersGroup("Admininstrators");

            UnitOfWork.Current.TransactionalFlush();

            UnitOfWork.CurrentSession.Evict(group);

            UsersGroup groupFromDb = Repository<UsersGroup>.Get(group.Id);
            Assert.IsNotNull(groupFromDb);
            Assert.AreEqual(group.Name, groupFromDb.Name);
        }

        [Test]
        public void CanCreateEntitesGroup()
        {
            EntitiesGroup group = authorizationRepository.CreateEntitiesGroup("Accounts");
            UnitOfWork.Current.TransactionalFlush();

            UnitOfWork.CurrentSession.Evict(group);

            EntitiesGroup groupFromDb = Repository<EntitiesGroup>.Get(group.Id);
            Assert.IsNotNull(groupFromDb);
            Assert.AreEqual(group.Name, groupFromDb.Name);
        }

        [Test]
        [ExpectedException(typeof (ValidationException))]
        public void CannotCreateEntitiesGroupWithSameName()
        {
            authorizationRepository.CreateEntitiesGroup("Admininstrators");
            UnitOfWork.Current.TransactionalFlush();
            authorizationRepository.CreateEntitiesGroup("Admininstrators");
        }

        [Test]
        [ExpectedException(typeof (ValidationException))]
        public void CannotCreateUsersGroupsWithSameName()
        {
            authorizationRepository.CreateUsersGroup("Admininstrators");
            UnitOfWork.Current.TransactionalFlush();
            authorizationRepository.CreateUsersGroup("Admininstrators");
        }

        [Test]
        public void CanGetUsersGroupByName()
        {
            UsersGroup group = authorizationRepository.CreateUsersGroup("Admininstrators");
            UnitOfWork.Current.TransactionalFlush();

            UnitOfWork.CurrentSession.Evict(group);

            group = authorizationRepository.GetUsersGroupByName("Admininstrators");
            Assert.IsNotNull(group);
        }

        [Test]
        public void CanGetEntitiesGroupByName()
        {
            EntitiesGroup group = authorizationRepository.CreateEntitiesGroup("Accounts");
            UnitOfWork.Current.TransactionalFlush();

            UnitOfWork.CurrentSession.Evict(group);

            group = authorizationRepository.GetEntitiesGroupByName("Accounts");
            Assert.IsNotNull(group);
        }

        [Test]
        public void CanChangeUsersGroupName()
        {
            UsersGroup group = authorizationRepository.CreateUsersGroup("Admininstrators");
            UnitOfWork.Current.TransactionalFlush();

            UnitOfWork.CurrentSession.Evict(group);

            authorizationRepository.RenameUsersGroup("Admininstrators", "2");
            UnitOfWork.Current.TransactionalFlush();

            UnitOfWork.CurrentSession.Evict(group);

            group = authorizationRepository.GetUsersGroupByName("2");
            Assert.IsNotNull(group);
            group = authorizationRepository.GetUsersGroupByName("Admininstrators");
            Assert.IsNull(group);
        }

        [Test]
        [ExpectedException(typeof(ValidationException))]
        public void CannotRenameUsersGroupToAnAlreadyExistingUsersGroup()
        {
            UsersGroup group = authorizationRepository.CreateUsersGroup("Admininstrators");
            UsersGroup group2 = authorizationRepository.CreateUsersGroup("ExistingGroup");

            UnitOfWork.Current.TransactionalFlush();

            UnitOfWork.CurrentSession.Evict(group);
            UnitOfWork.CurrentSession.Evict(group2);

            authorizationRepository.RenameUsersGroup("Admininstrators", "ExistingGroup");
        }

        [Test]
        public void CanChangeEntitiesGroupName()
        {
            EntitiesGroup group = authorizationRepository.CreateEntitiesGroup("Accounts");
            UnitOfWork.Current.TransactionalFlush();

            UnitOfWork.CurrentSession.Evict(group);

            authorizationRepository.RenameEntitiesGroup("Accounts", "2");            
            UnitOfWork.Current.TransactionalFlush();

            UnitOfWork.CurrentSession.Evict(group);

            group = authorizationRepository.GetEntitiesGroupByName("2");
            Assert.IsNotNull(group);
            group = authorizationRepository.GetEntitiesGroupByName("Accounts");
            Assert.IsNull(group);
        }

        [Test]
        [ExpectedException(typeof(ValidationException))]
        public void CannotRenameEntitiesGroupToAnAlreadyExistingEntitiesGroup()
        {
            EntitiesGroup group = authorizationRepository.CreateEntitiesGroup("Accounts");
            EntitiesGroup group2 = authorizationRepository.CreateEntitiesGroup("ExistingGroup");

            UnitOfWork.Current.TransactionalFlush();

            UnitOfWork.CurrentSession.Evict(group);
            UnitOfWork.CurrentSession.Evict(group2);

            authorizationRepository.RenameEntitiesGroup("Accounts", "ExistingGroup");        
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), "There is no users group named: NonExistingGroup")]
        public void CannotRenameUsersGroupThatDoesNotExist()
        {
            authorizationRepository.RenameUsersGroup("NonExistingGroup", "Administrators");    
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), "There is no entities group named: NonExistingGroup")]
        public void CannotRenameEntitiesGroupThatDoesNotExist()
        {
            authorizationRepository.RenameEntitiesGroup("NonExistingGroup", "Accounts");
        }


        [Test]
        public void CanAssociateUserWithGroup()
        {
            User ayende = new User();
            ayende.Name = "ayende";

            UnitOfWork.CurrentSession.Save(ayende);
            UsersGroup group = authorizationRepository.CreateUsersGroup("Admins");
            UnitOfWork.Current.TransactionalFlush();

            authorizationRepository.AssociateUserWith(ayende, "Admins");
            UnitOfWork.Current.TransactionalFlush();

            UnitOfWork.CurrentSession.Evict(ayende);
            UnitOfWork.CurrentSession.Evict(group);

            UsersGroup[] groups = authorizationRepository.GetAssociatedUsersGroupFor(ayende);
            Assert.AreEqual(1, groups.Length);
            Assert.AreEqual("Admins", groups[0].Name);
        }

        [Test]
        public void CanAssociateAccountWithMultipleGroups()
        {
            Account ayende = new Account();
            ayende.Name = "ayende";

            UnitOfWork.CurrentSession.Save(ayende);
            EntitiesGroup group = authorizationRepository.CreateEntitiesGroup("Accounts");
            EntitiesGroup group2 = authorizationRepository.CreateEntitiesGroup("Accounts of second group");
            UnitOfWork.Current.TransactionalFlush();

            authorizationRepository.AssociateEntityWith(ayende, "Accounts");
            UnitOfWork.Current.TransactionalFlush();
            authorizationRepository.AssociateEntityWith(ayende, "Accounts of second group");

            UnitOfWork.Current.TransactionalFlush();

            UnitOfWork.CurrentSession.Evict(ayende);
            UnitOfWork.CurrentSession.Evict(group);
            UnitOfWork.CurrentSession.Evict(group2);

            EntitiesGroup[] groups = authorizationRepository.GetAssociatedEntitiesGroupsFor(ayende);
            Assert.AreEqual(2, groups.Length);
            Assert.AreEqual("Accounts", groups[0].Name);
            Assert.AreEqual("Accounts of second group", groups[1].Name);
        }

        [Test]
        public void CanAssociateUserWithNestedGroup()
        {
            User ayende = new User();
            ayende.Name = "ayende";

            UnitOfWork.CurrentSession.Save(ayende);
            authorizationRepository.CreateUsersGroup("Admins");
            UnitOfWork.Current.TransactionalFlush();
            UsersGroup group = authorizationRepository.CreateChildUserGroupOf("Admins", "DBA");
            UnitOfWork.Current.TransactionalFlush();

            authorizationRepository.AssociateUserWith(ayende, "DBA");
            UnitOfWork.Current.TransactionalFlush();

            UnitOfWork.CurrentSession.Evict(ayende);
            UnitOfWork.CurrentSession.Evict(group);

            UsersGroup[] groups = authorizationRepository.GetAssociatedUsersGroupFor(ayende);
            Assert.AreEqual(2, groups.Length);
            Assert.AreEqual("Admins", groups[0].Name);
            Assert.AreEqual("DBA", groups[1].Name);
        }


        [Test]
        public void CanGetAncestryAssociationOfUserWithGroupWithNested()
        {
            User ayende = new User();
            ayende.Name = "ayende";

            UnitOfWork.CurrentSession.Save(ayende);
            authorizationRepository.CreateUsersGroup("Admins");
            UnitOfWork.Current.TransactionalFlush();
            authorizationRepository.CreateChildUserGroupOf("Admins", "DBA");
            UnitOfWork.Current.TransactionalFlush();

            authorizationRepository.AssociateUserWith(ayende, "DBA");
            UnitOfWork.Current.TransactionalFlush();

            UsersGroup[] groups = authorizationRepository.GetAncestryAssociation(ayende, "Admins");
            Assert.AreEqual(2, groups.Length);
            Assert.AreEqual("DBA", groups[0].Name);
            Assert.AreEqual("Admins", groups[1].Name);
        }

        [Test]
        public void CanGetAncestryAssociationOfUserWithGroupDirect()
        {
            User ayende = new User();
            ayende.Name = "ayende";

            UnitOfWork.CurrentSession.Save(ayende);
            authorizationRepository.CreateUsersGroup("Admins");
            UnitOfWork.Current.TransactionalFlush();

            authorizationRepository.AssociateUserWith(ayende, "Admins");
            UnitOfWork.Current.TransactionalFlush();

            UsersGroup[] groups = authorizationRepository.GetAncestryAssociation(ayende, "Admins");
            Assert.AreEqual(1, groups.Length);
            Assert.AreEqual("Admins", groups[0].Name);
        }

        [Test]
        public void CanGetAncestryAssociationOfUserWithGroupWhereNonExists()
        {
            User ayende = new User();
            ayende.Name = "ayende";

            UnitOfWork.CurrentSession.Save(ayende);
            authorizationRepository.CreateUsersGroup("Admins");
            UnitOfWork.Current.TransactionalFlush();


            UsersGroup[] groups = authorizationRepository.GetAncestryAssociation(ayende, "Admins");
            Assert.AreEqual(0, groups.Length);
        }

        [Test]
        public void CanGetAncestryAssociationOfUserWithGroupWhereThereIsDirectPathShouldSelectThat()
        {
            User ayende = new User();
            ayende.Name = "ayende";

            UnitOfWork.CurrentSession.Save(ayende);
            authorizationRepository.CreateUsersGroup("Admins");
            UnitOfWork.Current.TransactionalFlush();
            UnitOfWork.Current.TransactionalFlush();
            authorizationRepository.CreateChildUserGroupOf("Admins", "DBA");
            UnitOfWork.Current.TransactionalFlush();
            authorizationRepository.AssociateUserWith(ayende, "Admins");
            authorizationRepository.AssociateUserWith(ayende, "DBA");
            UnitOfWork.Current.TransactionalFlush();

            UsersGroup[] groups = authorizationRepository.GetAncestryAssociation(ayende, "Admins");
            Assert.AreEqual(1, groups.Length);
            Assert.AreEqual("Admins", groups[0].Name);
        }

        [Test]
        public void CanGetAncestryAssociationOfUserWithGroupWhereThereIsTwoLevelNesting()
        {
            User ayende = new User();
            ayende.Name = "ayende";

            UnitOfWork.CurrentSession.Save(ayende);
            authorizationRepository.CreateUsersGroup("Admins");
            UnitOfWork.Current.TransactionalFlush();
            UnitOfWork.Current.TransactionalFlush();
            authorizationRepository.CreateChildUserGroupOf("Admins", "DBA");
            UnitOfWork.Current.TransactionalFlush();
            authorizationRepository.CreateChildUserGroupOf("DBA", "SQLite DBA");
            UnitOfWork.Current.TransactionalFlush();
            authorizationRepository.AssociateUserWith(ayende, "SQLite DBA");
            UnitOfWork.Current.TransactionalFlush();

            UsersGroup[] groups = authorizationRepository.GetAncestryAssociation(ayende, "Admins");
            Assert.AreEqual(3, groups.Length);
            Assert.AreEqual("SQLite DBA", groups[0].Name);
            Assert.AreEqual("DBA", groups[1].Name);
            Assert.AreEqual("Admins", groups[2].Name);
        }

        [Test]
        public void CanGetAncestryAssociationOfUserWithGroupWhereThereIsMoreThanOneIndirectPathShouldSelectShortest()
        {
            User ayende = new User();
            ayende.Name = "ayende";

            UnitOfWork.CurrentSession.Save(ayende);
            authorizationRepository.CreateUsersGroup("Admins");
            UnitOfWork.Current.TransactionalFlush();
            UnitOfWork.Current.TransactionalFlush();
            authorizationRepository.CreateChildUserGroupOf("Admins", "DBA");
            UnitOfWork.Current.TransactionalFlush();
            authorizationRepository.CreateChildUserGroupOf("DBA", "SQLite DBA");
            UnitOfWork.Current.TransactionalFlush();
            authorizationRepository.AssociateUserWith(ayende, "DBA");
            authorizationRepository.AssociateUserWith(ayende, "SQLite DBA");
            UnitOfWork.Current.TransactionalFlush();

            UsersGroup[] groups = authorizationRepository.GetAncestryAssociation(ayende, "Admins");
            Assert.AreEqual(2, groups.Length);
            Assert.AreEqual("DBA", groups[0].Name);
            Assert.AreEqual("Admins", groups[1].Name);
        }

        [Test]
        public void CanAssociateAccountWithGroup()
        {
            Account ayende = new Account();
            ayende.Name = "ayende";

            UnitOfWork.CurrentSession.Save(ayende);
            EntitiesGroup group = authorizationRepository.CreateEntitiesGroup("Accounts");
            UnitOfWork.Current.TransactionalFlush();

            authorizationRepository.AssociateEntityWith(ayende, "Accounts");

            UnitOfWork.Current.TransactionalFlush();

            UnitOfWork.CurrentSession.Evict(ayende);
            UnitOfWork.CurrentSession.Evict(group);

            EntitiesGroup[] groups = authorizationRepository.GetAssociatedEntitiesGroupsFor(ayende);
            Assert.AreEqual(1, groups.Length);
            Assert.AreEqual("Accounts", groups[0].Name);
        }

        [Test]
        public void CanCreateOperation()
        {
            authorizationRepository.CreateOperation("/Account/Delete");
            UnitOfWork.Current.TransactionalFlush();
            Operation operation = authorizationRepository.GetOperationByName("/Account/Delete");
            Assert.IsNotNull(operation, "Could not create operation");
        }

        [Test]
        public void WhenCreatingNestedOperation_WillCreateParentOperation_IfDoesNotExists()
        {
            Operation operation = authorizationRepository.CreateOperation("/Account/Delete");
            UnitOfWork.Current.TransactionalFlush();
            Operation parentOperation = authorizationRepository.GetOperationByName("/Account");
            Assert.IsNotNull(parentOperation);
            Assert.AreEqual(operation.Parent, parentOperation);
        }

        [Test]
        public void WhenCreatingNestedOperation_WillLinkToParentOperation()
        {
            authorizationRepository.CreateOperation("/Account/Delete");
            UnitOfWork.Current.TransactionalFlush();
            Operation parentOperation = authorizationRepository.GetOperationByName("/Account");
            Assert.IsNotNull(parentOperation); // was created in setup
            Assert.AreEqual(2, parentOperation.Children.Count); // /Edit, /Delete
        }

        [Test]
        public void CanRemoveUserGroup()
        {
            authorizationRepository.RemoveUsersGroup("Administrators");
            UnitOfWork.Current.TransactionalFlush();

            Assert.IsNull(authorizationRepository.GetUsersGroupByName("Administrators"));
        }

        [Test]
        [ExpectedException(typeof (InvalidOperationException),
            "Cannot remove users group 'Administrators' because is has child groups. Remove those groups and try again."
            )]
        public void RemovingParentUserGroupWillFail()
        {
            authorizationRepository.CreateChildUserGroupOf("Administrators", "DBA");
            UnitOfWork.Current.TransactionalFlush();
            authorizationRepository.RemoveUsersGroup("Administrators");
        }


        [Test]
        public void WhenRemovingUsersGroupThatHasAssociatedPermissionsThoseShouldBeRemoved()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For("Administrators")
                .OnEverything()
                .DefaultLevel()
                .Save();
            UnitOfWork.Current.TransactionalFlush();

            Permission[] permissions = permissionService.GetPermissionsFor(user);
            Assert.IsNotEmpty(permissions);

            authorizationRepository.RemoveUsersGroup("Administrators");
            UnitOfWork.Current.TransactionalFlush();

            permissions = permissionService.GetPermissionsFor(user);
            Assert.IsEmpty(permissions);
        }

        [Test]
        public void CanRemoveNestedUserGroup()
        {
            UsersGroup dbaGroup = authorizationRepository.CreateChildUserGroupOf("Administrators", "DBA");
            UnitOfWork.Current.TransactionalFlush();

            authorizationRepository.RemoveUsersGroup("DBA");
            UnitOfWork.Current.TransactionalFlush();

            Assert.IsNull(authorizationRepository.GetUsersGroupByName("DBA"));

            UsersGroup administratorsGroup = 
                authorizationRepository.GetUsersGroupByName("Administrators");
            Assert.AreEqual(0,
                            administratorsGroup.DirectChildren.Count
                );
            Assert.AreEqual(0,
                            administratorsGroup.AllChildren.Count
                );

            Assert.AreEqual(0, dbaGroup.AllParents.Count);
        }

        [Test]
        public void UsersAreNotAssociatedWithRemovedGroups()
        {
            authorizationRepository.CreateChildUserGroupOf("Administrators", "DBA");
            UnitOfWork.Current.TransactionalFlush();

            authorizationRepository.AssociateUserWith(user, "DBA");
            UnitOfWork.Current.TransactionalFlush();

            UsersGroup[] associedGroups = authorizationRepository.GetAssociatedUsersGroupFor(user);
            Assert.AreEqual(2, associedGroups.Length);

            authorizationRepository.RemoveUsersGroup("DBA");
            UnitOfWork.Current.TransactionalFlush();


            associedGroups = authorizationRepository.GetAssociatedUsersGroupFor(user);
            Assert.AreEqual(1, associedGroups.Length);
        }

        [Test]
        public void CanRemoveEntitiesGroup()
        {
            authorizationRepository.RemoveEntitiesGroup("Important Accounts");
            UnitOfWork.Current.TransactionalFlush();
            Assert.IsNull(authorizationRepository.GetEntitiesGroupByName("Important Accounts")); ;
        }


        [Test]
        public void WhenRemovingEntitiesGroupAllPermissionsOnItWillBeDeleted()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(user)
                .On("Important Accounts")
                .DefaultLevel()
                .Save();
            UnitOfWork.Current.TransactionalFlush();
       
            Permission[] permissions = permissionService.GetPermissionsFor(user);
            Assert.IsNotEmpty(permissions);

            authorizationRepository.RemoveEntitiesGroup("Important Accounts");
            UnitOfWork.Current.TransactionalFlush();

            permissions = permissionService.GetPermissionsFor(user);
            Assert.IsEmpty(permissions);
        }

        [Test]
        public void CanRemoveOperation()
        {
            authorizationRepository.RemoveOperation("/Account/Edit");
            UnitOfWork.Current.TransactionalFlush();
            Assert.IsNull(authorizationRepository.GetOperationByName("/Account/Edit"));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), "Cannot remove operation '/Account' because it has child operations. Remove those operations and try again.")]
        public void CannotRemoveParentOperatio()
        {
            authorizationRepository.RemoveOperation("/Account");
        }

        [Test]
        public void CanRemoveNestedOperation()
        {
            authorizationRepository.RemoveOperation("/Account/Edit");
            UnitOfWork.Current.TransactionalFlush();
            Operation parent = authorizationRepository.GetOperationByName("/Account");

            Assert.AreEqual(0, parent.Children.Count);
        }

        [Test]
        public void CanRemoveUser()
        {
            authorizationRepository.RemoveUser(user);
            Repository<User>.Delete(user);
            UnitOfWork.Current.TransactionalFlush();
        }

        [Test]
        public void RemovingUserWillAlsoRemoveAssociatedPermissions()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(user)
                .OnEverything()
                .DefaultLevel()
                .Save();
            UnitOfWork.Current.TransactionalFlush();
            authorizationRepository.RemoveUser(user);
            Repository<User>.Delete(user);
            UnitOfWork.Current.TransactionalFlush();
        }
    }
}