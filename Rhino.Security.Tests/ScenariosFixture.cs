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
        public void CanOnlyAssignCasesThatAreAssignedToMe()
        {
            Operation operation = authorizationEditingService.CreateOperation("/Case/Assign");
            EntitiesGroup group = authorizationEditingService.CreateEntitiesGroup("Assigned to " + user.Name);
            permissionsBuilderService
                .Allow(operation)
                .For(user)
                .On(group)
                .DefaultLevel()
                .Save();

            authorizationEditingService.AssociateEntityWith(account, group);
            UnitOfWork.Current.TransactionalFlush();

            bool allowed = authorizationService.IsAllowed(user, account, "/Case/Assign");
            Assert.IsTrue(allowed);
        }
    }
}