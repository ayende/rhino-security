namespace Rhino.Security.Tests
{
    using System;
    using Commons;
    using MbUnit.Framework;

    [TestFixture]
    public class ScenariosFixture : DatabaseFixture
    {
        [Test]
        public void DeeplyNestedUsersGroup()
        {
            UsersGroup group = authorizationEditingService.CreateUsersGroup("Root");
            UnitOfWork.Current.Flush();
            for (int j = 0; j < 50; j++)
            {
                group = authorizationEditingService.CreateChildUserGroupOf(group.Name, "Child #" + j);
                UnitOfWork.Current.Flush();
            }
            UnitOfWork.Current.TransactionalFlush();

            authorizationEditingService.AssociateUserWith(user, "Child #49");
            UnitOfWork.Current.TransactionalFlush();
            UsersGroup[] groups = authorizationEditingService.GetAncestryAssociation(user, "Root");
            Assert.AreEqual(51, groups.Length);
        }

        [Test]
        public void CanOnlyAssignAccountsThatAreAssignedToMe()
        {
            // during first deploy
            Operation operation = authorizationEditingService.CreateOperation("/Account/Assign");

            User secondUser = new User();
            secondUser.Name = "Second user";
            Repository<User>.Save(secondUser);

            // setup entities group for the users
            AddDefaultPermissions(operation, secondUser);
            AddDefaultPermissions(operation, user);

            UnitOfWork.Current.TransactionalFlush();

            authorizationEditingService.AssociateEntityWith(account, "Assigned to " + user.Name);
            UnitOfWork.Current.TransactionalFlush();

            // validate that I can assign a case
            bool allowed = authorizationService.IsAllowed(user, account, "/Account/Assign");
            Assert.IsTrue(allowed);

            // validate that second cannot
            allowed = authorizationService.IsAllowed(secondUser, account, "/Account/Assign");
            Assert.IsFalse(allowed);

            // the act of assigning is simply moving from one entity group to another
            authorizationEditingService.DetachEntityFromGroup(account, "Assigned to " + user.Name);
            authorizationEditingService.AssociateEntityWith(account, "Assigned to " + secondUser.Name);

            // have to commit the transaction for it to work
            UnitOfWork.Current.TransactionalFlush();

            // validate that I can not longer assign a case
            allowed = authorizationService.IsAllowed(user, account, "/Account/Assign");
            Assert.IsFalse(allowed);

            // validate that second now can assign
            allowed = authorizationService.IsAllowed(secondUser, account, "/Account/Assign");
            Assert.IsTrue(allowed);
        }

        [Test]
        public void CanOnlyViewAccountsThatUserBelongsTo()
        {
            // on first deploy
            Operation operation = authorizationEditingService.CreateOperation("/Account/View");
            // when creating account
            UsersGroup group = authorizationEditingService.CreateUsersGroup("Belongs to " + account.Name);
            UnitOfWork.Current.TransactionalFlush();

            // setting permission so only associated users can view
            permissionsBuilderService
                .Allow(operation)
                .For(group)
                .On(account)
                .DefaultLevel()
                .Save();
            UnitOfWork.Current.TransactionalFlush();

            // when adding user to account
            authorizationEditingService.AssociateUserWith(user, group);
            UnitOfWork.Current.TransactionalFlush();
            
            bool allowed = authorizationService.IsAllowed(user, account, "/Account/View");
            Assert.IsTrue(allowed);
        }

        private void AddDefaultPermissions(Operation operation, User toUser)
        {
            EntitiesGroup group = authorizationEditingService.CreateEntitiesGroup("Assigned to " + toUser.Name);
            permissionsBuilderService
                .Allow(operation)
                .For(toUser)
                .On(group)
                .DefaultLevel()
                .Save();
        }
    }
}