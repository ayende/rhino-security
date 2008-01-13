namespace Rhino.Security.Tests
{
    using System;
    using Commons;
    using MbUnit.Framework;
    using NHibernate;
    using Rhino.Commons.ForTesting;

    [TestFixture]
    public class AuthorizationEditing_DAO_Fixture : DatabaseFixture
    {
        [Test]
        public void CanSaveUser()
        {
            using (ISession session = CurrentContext.CreateSession())
            {
                User user = new User();
                user.Name = "ayende";
                Assert.AreNotEqual(Guid.Empty, user.SecurityKey);
                session.Save(user);
                session.Flush();
                session.Evict(user);

                User fromDb = session.Get<User>(user.Id);
                Assert.IsNotNull(fromDb);
                Assert.AreEqual(user.Name, fromDb.Name);
                Assert.AreEqual(fromDb.SecurityKey, user.SecurityKey);
            }
        }

        [Test]
        public void CanSaveAccount()
        {
            using (ISession session = CurrentContext.CreateSession())
            {
                Account account = new Account();
                account.Name = "ayende";
                Assert.AreNotEqual(Guid.Empty, account.SecurityKey);
                session.Save(account);
                session.Flush();
                session.Evict(account);

                Account fromDb = session.Get<Account>(account.Id);
                Assert.IsNotNull(fromDb);
                Assert.AreEqual(account.Name, fromDb.Name);
                Assert.AreEqual(fromDb.SecurityKey, account.SecurityKey);
            }
        }

        [Test]
        public void CanCreateUsersGroup()
        {
            IAuthorizationEditingService authorizationEditingService = IoC.Resolve<IAuthorizationEditingService>();
            UsersGroup group = authorizationEditingService.CreateUsersGroup("Admininstrators");

            UnitOfWork.Current.TransactionalFlush();

            UnitOfWork.CurrentSession.Evict(group);

            UsersGroup groupFromDb = Repository<UsersGroup>.Get(group.Id);
            Assert.IsNotNull(groupFromDb);
            Assert.AreEqual(group.Name, groupFromDb.Name);
        }

        [Test]
        public void CanCreateEntitesGroup()
        {
            IAuthorizationEditingService authorizationEditingService = IoC.Resolve<IAuthorizationEditingService>();
            EntitiesGroup group = authorizationEditingService.CreateEntitiesGroup("Accounts");
            UnitOfWork.Current.TransactionalFlush();

            UnitOfWork.CurrentSession.Evict(group);

            EntitiesGroup groupFromDb = Repository<EntitiesGroup>.Get(group.Id);
            Assert.IsNotNull(groupFromDb);
            Assert.AreEqual(group.Name, groupFromDb.Name);
        }

        [Test]
        [ExpectedException(typeof(ValidationException))]
        public void CannotCreateEntitiesGroupWithSameName()
        {
            IAuthorizationEditingService authorizationEditingService = IoC.Resolve<IAuthorizationEditingService>();
            authorizationEditingService.CreateEntitiesGroup("Admininstrators");
            UnitOfWork.Current.TransactionalFlush();
            authorizationEditingService.CreateEntitiesGroup("Admininstrators");
        }

        [Test]
        [ExpectedException(typeof(ValidationException))]
        public void CannotCreateUsersGroupsWithSameName()
        {
            IAuthorizationEditingService authorizationEditingService = IoC.Resolve<IAuthorizationEditingService>();
            authorizationEditingService.CreateUsersGroup("Admininstrators");
            UnitOfWork.Current.TransactionalFlush();
            authorizationEditingService.CreateUsersGroup("Admininstrators");
        }

        [Test]
        public void CanGetUsersGroupByName()
        {
            IAuthorizationEditingService authorizationEditingService = IoC.Resolve<IAuthorizationEditingService>();
            UsersGroup group = authorizationEditingService.CreateUsersGroup("Admininstrators");
            UnitOfWork.Current.TransactionalFlush();

            UnitOfWork.CurrentSession.Evict(group);

            group = authorizationEditingService.GetUsersGroupByName("Admininstrators");
            Assert.IsNotNull(group);
        }

        [Test]
        public void CanGetEntitiesGroupByName()
        {
            IAuthorizationEditingService authorizationEditingService = IoC.Resolve<IAuthorizationEditingService>();
            EntitiesGroup group = authorizationEditingService.CreateEntitiesGroup("Accounts");
            UnitOfWork.Current.TransactionalFlush();

            UnitOfWork.CurrentSession.Evict(group);

            group = authorizationEditingService.GetEntitiesGroupByName("Accounts");
            Assert.IsNotNull(group);
        }

        [Test]
        public void CannotChangeUsersGroupName()
        {
            IAuthorizationEditingService authorizationEditingService = IoC.Resolve<IAuthorizationEditingService>();
            UsersGroup group = authorizationEditingService.CreateUsersGroup("Admininstrators");
            UnitOfWork.Current.TransactionalFlush();

            UnitOfWork.CurrentSession.Evict(group);

            group = authorizationEditingService.GetUsersGroupByName("Admininstrators");
            group.Name = "2";
            UnitOfWork.Current.TransactionalFlush();

            UnitOfWork.CurrentSession.Evict(group);

            group = authorizationEditingService.GetUsersGroupByName("2");
            Assert.IsNull(group);
            group = authorizationEditingService.GetUsersGroupByName("Admininstrators");
            Assert.IsNotNull(group);
        }

        [Test]
        public void CannotChangeEntitiesGroupName()
        {
            IAuthorizationEditingService authorizationEditingService = IoC.Resolve<IAuthorizationEditingService>();
            EntitiesGroup group = authorizationEditingService.CreateEntitiesGroup("Accounts");
            UnitOfWork.Current.TransactionalFlush();

            UnitOfWork.CurrentSession.Evict(group);

            group = authorizationEditingService.GetEntitiesGroupByName("Accounts");
            group.Name = "2";
            UnitOfWork.Current.TransactionalFlush();

            UnitOfWork.CurrentSession.Evict(group);

            group = authorizationEditingService.GetEntitiesGroupByName("2");
            Assert.IsNull(group);
            group = authorizationEditingService.GetEntitiesGroupByName("Accounts");
            Assert.IsNotNull(group);
        }

        [Test]
        public void CanAssociateUserWithGroup()
        {
            IAuthorizationEditingService authorizationEditingService = IoC.Resolve<IAuthorizationEditingService>();
            using (ISession session = CurrentContext.CreateSession())
            {
                User user = new User();
                user.Name = "ayende";

                session.Save(user);
                UsersGroup group = authorizationEditingService.CreateUsersGroup("Admins");
                UnitOfWork.Current.TransactionalFlush();

                authorizationEditingService.AssociateUserWith(user, "Admins");
                UnitOfWork.Current.TransactionalFlush();

                session.Evict(user);
                session.Evict(group);

                UsersGroup[] groups = authorizationEditingService.GetAssociatedUsersGroupFor(user);
                Assert.AreEqual(1, groups.Length);
                Assert.AreEqual("Admins", groups[0].Name);
            }
        }

        [Test]
        public void CanAssociateAccountWithGroup()
        {
            IAuthorizationEditingService authorizationEditingService = IoC.Resolve<IAuthorizationEditingService>();
            using (ISession session = CurrentContext.CreateSession())
            {
                Account account = new Account();
                account.Name = "ayende";

                session.Save(account);
                EntitiesGroup group = authorizationEditingService.CreateEntitiesGroup("Accounts");
                UnitOfWork.Current.TransactionalFlush();

                authorizationEditingService.AssociateEntityWith(account, "Accounts");

                UnitOfWork.Current.TransactionalFlush();

                session.Evict(account);
                session.Evict(group);

                EntitiesGroup[] groups = authorizationEditingService.GetAssociatedEntitiesGroupsFor(account);
                Assert.AreEqual(1, groups.Length);
                Assert.AreEqual("Accounts", groups[0].Name);
            }
        }

        [Test]
        public void CanCreateOperation()
        {
            IAuthorizationEditingService authorizationEditingService = IoC.Resolve<IAuthorizationEditingService>();
            authorizationEditingService.CreateOperation("/Account/Edit");
            UnitOfWork.Current.TransactionalFlush();
            Operation operation = authorizationEditingService.GetOperationByName("/Account/Edit");
            Assert.IsNotNull(operation);
        }

        [Test]
        public void WhenCreatingNestedOperation_WillCreateParentOperation_IfDoesNotExists()
        {
            IAuthorizationEditingService authorizationEditingService = IoC.Resolve<IAuthorizationEditingService>();
            Operation operation = authorizationEditingService.CreateOperation("/Account/Edit");
            UnitOfWork.Current.TransactionalFlush();
            Operation parentOperation = authorizationEditingService.GetOperationByName("/Account");
            Assert.IsNotNull(parentOperation);
            Assert.AreEqual(operation.Parent, parentOperation);
        }

        [Test]
        public void WhenCreatingNestedOperation_WillLinkToParentOperation()
        {
            IAuthorizationEditingService authorizationEditingService = IoC.Resolve<IAuthorizationEditingService>();
            authorizationEditingService.CreateOperation("/Account");
            UnitOfWork.Current.TransactionalFlush();
            authorizationEditingService.CreateOperation("/Account/Edit");
            UnitOfWork.Current.TransactionalFlush();
            Operation parentOperation = authorizationEditingService.GetOperationByName("/Account");
            Assert.IsNotNull(parentOperation);
            Assert.AreEqual(1, parentOperation.Children.Count);
        }
    }
}