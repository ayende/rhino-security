using System;
using Xunit;

namespace Rhino.Security.Tests
{
    public class AuthorizationService_Explainations_Fixture : DatabaseFixture
    {
        [Fact]
        public void ExplainWhyAllowedWhenAllowedPermissionWasDefinedWithDenyPermissionWithLowerLevel()
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
            

            AuthorizationInformation information =
                authorizationService.GetAuthorizationInformation(user, "/Account/Edit");
            string expected =
                @"Permission (level 10) for operation '/Account/Edit' was granted to 'Ayende' on 'everything'
Permission (level 5) for operation '/Account/Edit' was denied to group 'Administrators' on 'everything' ('Ayende' is a member of 'Administrators')
";
			Assert.Equal(expected.TrimAndFixLineEndings(), information.ToString().TrimAndFixLineEndings());
        }

        [Fact]
        public void ExplainWhyNotAllowedIfOperationNotDefined()
        {
            AuthorizationInformation information =
                authorizationService.GetAuthorizationInformation(user, "/Account/Delete");
			Assert.Equal("Operation '/Account/Delete' was not defined".TrimAndFixLineEndings(), information.ToString().TrimAndFixLineEndings());
        }

        [Fact]
        public void ExplainWhyNotAllowedIfNoPermissionGranted()
        {
            AuthorizationInformation information =
                authorizationService.GetAuthorizationInformation(user, "/Account/Edit");
            Assert.Equal(
				"Permission for operation '/Account/Edit' was not granted to user 'Ayende' or to the groups 'Ayende' is associated with ('Administrators')".TrimAndFixLineEndings(),
				information.ToString().TrimAndFixLineEndings());
        }

        [Fact]
        public void ExplainWhyNotAllowedIfDenyPermissionWasDefined()
        {
            permissionsBuilderService
                .Deny("/Account/Edit")
                .For(user)
                .OnEverything()
                .DefaultLevel()
                .Save();
            

            AuthorizationInformation information =
                authorizationService.GetAuthorizationInformation(user, "/Account/Edit");
            Assert.Equal(
				"Permission (level 1) for operation '/Account/Edit' was denied to 'Ayende' on 'everything'".TrimAndFixLineEndings(),
				information.ToString().TrimAndFixLineEndings());
        }

        [Fact]
        public void ExplainWhyAllowedIfAllowPermissionWasDefined()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(user)
                .OnEverything()
                .DefaultLevel()
                .Save();
            

            AuthorizationInformation information =
                authorizationService.GetAuthorizationInformation(user, "/Account/Edit");
            Assert.Equal(
				"Permission (level 1) for operation '/Account/Edit' was granted to 'Ayende' on 'everything'".TrimAndFixLineEndings(),
				information.ToString().TrimAndFixLineEndings());
        }

        [Fact]
        public void ExplainWhyAllowedIfAllowPermissionWasDefinedOnGroup()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For("Administrators")
                .OnEverything()
                .DefaultLevel()
                .Save();
            

            AuthorizationInformation information =
                authorizationService.GetAuthorizationInformation(user, "/Account/Edit");
            Assert.Equal(
				"Permission (level 1) for operation '/Account/Edit' was granted to group 'Administrators' on 'everything' ('Ayende' is a member of 'Administrators')".TrimAndFixLineEndings(),
				information.ToString().TrimAndFixLineEndings());
        }


        [Fact]
        public void ExplainWhyNotAllowedIfbDenyPermissionWasDefinedOnGroup()
        {
            permissionsBuilderService
                .Deny("/Account/Edit")
                .For("Administrators")
                .OnEverything()
                .DefaultLevel()
                .Save();
            

            AuthorizationInformation information =
                authorizationService.GetAuthorizationInformation(user, "/Account/Edit");
            Assert.Equal(
				"Permission (level 1) for operation '/Account/Edit' was denied to group 'Administrators' on 'everything' ('Ayende' is a member of 'Administrators')".TrimAndFixLineEndings(),
				information.ToString().TrimAndFixLineEndings());
        }


        [Fact]
        public void ExplainWhyNotAllowedIfAllowPermissionWasDefinedOnGroupAndDenyPermissionOnUser()
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
            

            AuthorizationInformation information =
                authorizationService.GetAuthorizationInformation(user, "/Account/Edit");
            string expected =
                @"Permission (level 1) for operation '/Account/Edit' was denied to group 'Administrators' on 'everything' ('Ayende' is a member of 'Administrators')
Permission (level 1) for operation '/Account/Edit' was granted to 'Ayende' on 'everything'
";
			Assert.Equal(expected.TrimAndFixLineEndings(), information.ToString().TrimAndFixLineEndings());
        }

        [Fact]
        public void ExplainWhyNotAllowedWhenAllowedPermissionWasDefinedWithDenyPermissionWithHigherLevel()
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
            

            AuthorizationInformation information =
                authorizationService.GetAuthorizationInformation(user, "/Account/Edit");
            string expected =
                @"Permission (level 5) for operation '/Account/Edit' was denied to group 'Administrators' on 'everything' ('Ayende' is a member of 'Administrators')
Permission (level 1) for operation '/Account/Edit' was granted to 'Ayende' on 'everything'
";
			Assert.Equal(expected.TrimAndFixLineEndings(), information.ToString().TrimAndFixLineEndings());
        }

        [Fact]
        public void ExplainWhyAllowedOnAccountIfPermissionWasGrantedOnAnything()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(user)
                .OnEverything()
                .DefaultLevel()
                .Save();
            

            AuthorizationInformation information =
                authorizationService.GetAuthorizationInformation(user, account, "/Account/Edit");
            string expected =
                @"Permission (level 1) for operation '/Account/Edit' was granted to 'Ayende' on 'everything'
";
			Assert.Equal(expected.TrimAndFixLineEndings(), information.ToString().TrimAndFixLineEndings());
        }


        [Fact]
        public void ExplainWhyDeniedOnAccountIfPermissionWasDeniedOnAnything()
        {
            permissionsBuilderService
                .Deny("/Account/Edit")
                .For(user)
                .OnEverything()
                .DefaultLevel()
                .Save();
            

            AuthorizationInformation information =
                authorizationService.GetAuthorizationInformation(user, account, "/Account/Edit");
            string expected =
                @"Permission (level 1) for operation '/Account/Edit' was denied to 'Ayende' on 'everything'
";
			Assert.Equal(expected.TrimAndFixLineEndings(), information.ToString().TrimAndFixLineEndings());
        }

        [Fact]
        public void ExplainWhyAllowedOnAccountIfPermissionWasGrantedOnGroupAssociatedWithUser()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For("Administrators")
                .On(account)
                .DefaultLevel()
                .Save();
            

            AuthorizationInformation information =
                authorizationService.GetAuthorizationInformation(user, account, "/Account/Edit");
            string expected =
                @"Permission (level 1) for operation '/Account/Edit' was granted to group 'Administrators' on 'Account: south sand' ('Ayende' is a member of 'Administrators')
";
			Assert.Equal(expected.TrimAndFixLineEndings(), information.ToString().TrimAndFixLineEndings());
        }

        [Fact]
        public void ExplainWhyDeniedOnAccountIfPermissionWasDeniedOnGroupAssociatedWithUser()
        {
            permissionsBuilderService
                .Deny("/Account/Edit")
                .For("Administrators")
                .On(account)
                .DefaultLevel()
                .Save();
            

            AuthorizationInformation information =
                authorizationService.GetAuthorizationInformation(user, account, "/Account/Edit");
            string expected =
                @"Permission (level 1) for operation '/Account/Edit' was denied to group 'Administrators' on 'Account: south sand' ('Ayende' is a member of 'Administrators')
";
			Assert.Equal(expected.TrimAndFixLineEndings(), information.ToString().TrimAndFixLineEndings());
        }

        [Fact]
        public void ExplainWhyAllowedOnAccountIfPermissionWasGrantedToUser()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(user)
                .On(account)
                .DefaultLevel()
                .Save();
            

            AuthorizationInformation information =
                authorizationService.GetAuthorizationInformation(user, account, "/Account/Edit");
            string expected =
                @"Permission (level 1) for operation '/Account/Edit' was granted to 'Ayende' on 'Account: south sand'
";
			Assert.Equal(expected.TrimAndFixLineEndings(), information.ToString().TrimAndFixLineEndings());
        }


        [Fact]
        public void ExplainWhyDeniedOnAccountIfPermissionWasDeniedToUser()
        {
            permissionsBuilderService
                .Deny("/Account/Edit")
                .For(user)
                .On(account)
                .DefaultLevel()
                .Save();
            

            AuthorizationInformation information =
                authorizationService.GetAuthorizationInformation(user, account, "/Account/Edit");
            string expected =
                @"Permission (level 1) for operation '/Account/Edit' was denied to 'Ayende' on 'Account: south sand'
";
			Assert.Equal(expected.TrimAndFixLineEndings(), information.ToString().TrimAndFixLineEndings());
        }

        [Fact]
        public void ExplainWhyAllowedOnEntityGroupIfPermissionWasGrantedToUsersGroupAssociatedWithUser()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For("Administrators")
                .On("Important Accounts")
                .DefaultLevel()
                .Save();
            
            AuthorizationInformation information =
                authorizationService.GetAuthorizationInformation(user, account, "/Account/Edit");
            string expected =
                @"Permission (level 1) for operation '/Account/Edit' was granted to group 'Administrators' on ''Important Accounts' ('Account: south sand' is a member of 'Important Accounts')' ('Ayende' is a member of 'Administrators')
";
			Assert.Equal(expected.TrimAndFixLineEndings(), information.ToString().TrimAndFixLineEndings());
        }

        [Fact]
        public void ExplainWhyDeniedIfPermissionWasGrantedToEntitiesGroupButNotToGlobal()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For("Administrators")
                .On("Important Accounts")
                .DefaultLevel()
                .Save();
            
            AuthorizationInformation information =
                authorizationService.GetAuthorizationInformation(user, "/Account/Edit");
            string expected =
                @"Permission for operation '/Account/Edit' was not granted to user 'Ayende' or to the groups 'Ayende' is associated with ('Administrators')
";
			Assert.Equal(expected.TrimAndFixLineEndings(), information.ToString().TrimAndFixLineEndings());
        }

        [Fact]
        public void ExplainWhyDeniedOnAccountIfNoPermissionIsDefined()
        {
            AuthorizationInformation information =
                authorizationService.GetAuthorizationInformation(user, account, "/Account/Edit");
            string expected =
                @"Permission for operation '/Account/Edit' was not granted to user 'Ayende' or to the groups 'Ayende' is associated with ('Administrators') on 'Account: south sand' or any of the groups 'Account: south sand' is associated with ('Important Accounts')
";
			Assert.Equal(expected.TrimAndFixLineEndings(), information.ToString().TrimAndFixLineEndings());
        }
        //Todo: had to session flush for the test to pass.
        [Fact]
        public void ExplainWhyDeniedOnAccountWhenHaveNoGroupsOnUserOrEntity()
        {
            authorizationRepository.DetachUserFromGroup(user, "Administrators");
            authorizationRepository.DetachEntityFromGroup(account, "Important Accounts");
            
            session.Flush();

            AuthorizationInformation information =
                authorizationService.GetAuthorizationInformation(user, account, "/Account/Edit");
            string expected =
                @"Permission for operation '/Account/Edit' was not granted to user 'Ayende' or to the groups 'Ayende' is associated with ('not assoicated with any group') on 'Account: south sand' or any of the groups 'Account: south sand' is associated with ('not assoicated with any group')
";
			Assert.Equal(expected.TrimAndFixLineEndings(), information.ToString().TrimAndFixLineEndings());
        }

        [Fact]
        public void ExplainWhyDeniedOnAccountIfPermissionWasDeniedToUserOnTheGroupTheEntityIsAssociatedWith()
        {
            permissionsBuilderService
                .Deny("/Account/Edit")
                .For(user)
                .On("Important Accounts")
                .DefaultLevel()
                .Save();
            

            AuthorizationInformation information =
                authorizationService.GetAuthorizationInformation(user, account, "/Account/Edit");
            string expected =
                @"Permission (level 1) for operation '/Account/Edit' was denied to 'Ayende' on ''Important Accounts' ('Account: south sand' is a member of 'Important Accounts')'
";
			Assert.Equal(expected.TrimAndFixLineEndings(), information.ToString().TrimAndFixLineEndings());
        }

        [Fact]
        public void ExplainWhyDeniedOnAccountIfPermissionWasAllowedToUserOnTheGroupTheEntityIsAssociatedWith()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(user)
                .On("Important Accounts")
                .DefaultLevel()
                .Save();
            

            AuthorizationInformation information =
                authorizationService.GetAuthorizationInformation(user, account, "/Account/Edit");
            string expected =
                @"Permission (level 1) for operation '/Account/Edit' was granted to 'Ayende' on ''Important Accounts' ('Account: south sand' is a member of 'Important Accounts')'
";
			Assert.Equal(expected.TrimAndFixLineEndings(), information.ToString().TrimAndFixLineEndings());
        }

        [Fact]
        public void ExplainWhyDeniedIfPermissionWasAllowedToChildGroupUserIsAssociatedWith()
        {
            authorizationRepository.CreateChildUserGroupOf("Administrators", "Helpdesk");
            

            permissionsBuilderService
                .Allow("/Account/Edit")
                .For("Helpdesk")
                .On("Important Accounts")
                .DefaultLevel()
                .Save();
            

            AuthorizationInformation information =
                authorizationService.GetAuthorizationInformation(user, account, "/Account/Edit");
            string expected =
                @"Permission for operation '/Account/Edit' was not granted to user 'Ayende' or to the groups 'Ayende' is associated with ('Administrators') on 'Account: south sand' or any of the groups 'Account: south sand' is associated with ('Important Accounts')
";
			Assert.Equal(expected.TrimAndFixLineEndings(), information.ToString().TrimAndFixLineEndings());
        }

        [Fact]
        public void ExplainWhyAllowedIfPermissionWasAllowedToParentGroupUserIsAssociatedWith()
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
            

            AuthorizationInformation information =
                authorizationService.GetAuthorizationInformation(user, account, "/Account/Edit");
            string expected =
                @"Permission (level 1) for operation '/Account/Edit' was granted to group 'Administrators' on ''Important Accounts' ('Account: south sand' is a member of 'Important Accounts')' ('Ayende' is a member of 'Helpdesk -> Administrators')";
			Assert.Equal(expected.TrimAndFixLineEndings(), information.ToString().TrimAndFixLineEndings());
        }
    }

	public static class StringExt
	{
		public static string TrimAndFixLineEndings(this string s)
		{
			return s.Replace("\r\n", "\n").Replace("\n", Environment.NewLine).Trim();
		}
	}
}
