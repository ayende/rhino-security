using Rhino.Security.Model;
using Xunit;

namespace Rhino.Security.Tests
{
    public class ScenariosFixture : DatabaseFixture
    {
        [Fact]
        public void DeeplyNestedUsersGroup()
        {
            UsersGroup group = authorizationRepository.CreateUsersGroup("Root");

            for (int j = 0; j < 50; j++)
            {
                group = authorizationRepository.CreateChildUserGroupOf(group.Name, "Child #" + j);
            
            }
            

            authorizationRepository.AssociateUserWith(user, "Child #49");
            
            UsersGroup[] groups = authorizationRepository.GetAncestryAssociation(user, "Root");
            Assert.Equal(51, groups.Length);
        }

        [Fact]
        public void CanOnlyAssignAccountsThatAreAssignedToMe()
        {
            // during first deploy
            Operation operation = authorizationRepository.CreateOperation("/Account/Assign");

            User secondUser = new User();
            secondUser.Name = "Second user";
            session.Save(secondUser);

            // setup entities group for the users
            AddDefaultPermissions(operation, secondUser);
            AddDefaultPermissions(operation, user);


            authorizationRepository.AssociateEntityWith(account, "Assigned to " + user.Name);

            // validate that I can assign a case
            bool allowed = authorizationService.IsAllowed(user, account, "/Account/Assign");
            Assert.True(allowed);

            // validate that second cannot
            allowed = authorizationService.IsAllowed(secondUser, account, "/Account/Assign");
            Assert.False(allowed);

            // the act of assigning is simply moving from one entity group to another
            authorizationRepository.DetachEntityFromGroup(account, "Assigned to " + user.Name);
            authorizationRepository.AssociateEntityWith(account, "Assigned to " + secondUser.Name);

            // have to commit the transaction for it to work

            // validate that I can not longer assign a case
            allowed = authorizationService.IsAllowed(user, account, "/Account/Assign");
            Assert.False(allowed);

            // validate that second now can assign
            allowed = authorizationService.IsAllowed(secondUser, account, "/Account/Assign");
            Assert.True(allowed);
        }

        [Fact]
        public void CanOnlyViewAccountsThatUserBelongsTo()
        {
            // on first deploy
            Operation operation = authorizationRepository.CreateOperation("/Account/View");
            // when creating account
            UsersGroup group = authorizationRepository.CreateUsersGroup("Belongs to " + account.Name);

            // setting permission so only associated users can view
            permissionsBuilderService
                .Allow(operation)
                .For(group)
                .On(account)
                .DefaultLevel()
                .Save();

            // when adding user to account
            authorizationRepository.AssociateUserWith(user, group);
            
            bool allowed = authorizationService.IsAllowed(user, account, "/Account/View");
            Assert.True(allowed);
        }

        private void AddDefaultPermissions(Operation operation, User toUser)
        {
            EntitiesGroup group = authorizationRepository.CreateEntitiesGroup("Assigned to " + toUser.Name);
            permissionsBuilderService
                .Allow(operation)
                .For(toUser)
                .On(group)
                .DefaultLevel()
                .Save();
        }
    }
}