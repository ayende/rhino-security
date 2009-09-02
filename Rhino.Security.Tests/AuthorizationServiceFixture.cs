using Rhino.Security.Model;
using Xunit;

namespace Rhino.Security.Tests
{
	public class AuthorizationServiceFixture : DatabaseFixture
	{
		[Fact]
		public void WillReturnFalseIfNoPermissionHasBeenDefined()
		{
			bool isAllowed = authorizationService.IsAllowed(user, "/Account/Edit");
			Assert.False(isAllowed);
		}

		[Fact]
		public void WillReturnFalseIfOperationNotDefined()
		{
			bool isAllowed = authorizationService.IsAllowed(user, "/Account/Delete");
			Assert.False(isAllowed);
		}

		[Fact]
		public void WillReturnTrueIfAllowPermissionWasDefined()
		{
			permissionsBuilderService
				.Allow("/Account/Edit")
				.For(user)
				.OnEverything()
				.DefaultLevel()
				.Save();
			

			bool isAllowed = authorizationService.IsAllowed(user, "/Account/Edit");
			Assert.True(isAllowed);
		}

		[Fact]
		public void WillReturnFalseIfAllowPermissionWasDefinedOnGroupAndDenyPermissionOnUser()
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
			

			bool isAllowed = authorizationService.IsAllowed(user, "/Account/Edit");
			Assert.False(isAllowed);
		}

		[Fact]
		public void WillReturnFalseIfAllowedPermissionWasDefinedWithDenyPermissionWithHigherLevel()
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
			

			bool isAllowed = authorizationService.IsAllowed(user, "/Account/Edit");
			Assert.False(isAllowed);
		}

		[Fact]
		public void WillReturnTrueIfAllowedPermissionWasDefinedWithDenyPermissionWithLowerLevel()
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
			

			bool isAllowed = authorizationService.IsAllowed(user, "/Account/Edit");
			Assert.True(isAllowed);
		}


		[Fact]
		public void WillReturnTrueOnAccountIfPermissionWasGrantedOnAnything()
		{
			permissionsBuilderService
				.Allow("/Account/Edit")
				.For(user)
				.OnEverything()
				.DefaultLevel()
				.Save();
			

			bool isAllowed = authorizationService.IsAllowed(user, account, "/Account/Edit");
			Assert.True(isAllowed);
		}

		[Fact]
		public void WillReturnFalseOnAccountIfPermissionWasDeniedOnAnything()
		{
			permissionsBuilderService
				.Deny("/Account/Edit")
				.For(user)
				.OnEverything()
				.DefaultLevel()
				.Save();
			

			bool isAllowed = authorizationService.IsAllowed(user, account, "/Account/Edit");
			Assert.False(isAllowed);
		}

		[Fact]
		public void WillReturnTrueOnAccountIfPermissionWasGrantedOnGroupAssociatedWithUser()
		{
			permissionsBuilderService
				.Allow("/Account/Edit")
				.For("Administrators")
				.On(account)
				.DefaultLevel()
				.Save();
			

			bool isAllowed = authorizationService.IsAllowed(user, account, "/Account/Edit");
			Assert.True(isAllowed);
		}

		[Fact]
		public void WillReturnFalseOnAccountIfPermissionWasDeniedOnGroupAssociatedWithUser()
		{
			permissionsBuilderService
				.Deny("/Account/Edit")
				.For("Administrators")
				.On(account)
				.DefaultLevel()
				.Save();
			

			bool isAllowed = authorizationService.IsAllowed(user, account, "/Account/Edit");
			Assert.False(isAllowed);
		}

		[Fact]
		public void WillReturnTrueOnAccountIfPermissionWasGrantedToUser()
		{
			permissionsBuilderService
				.Allow("/Account/Edit")
				.For(user)
				.On(account)
				.DefaultLevel()
				.Save();
			

			bool isAllowed = authorizationService.IsAllowed(user, account, "/Account/Edit");
			Assert.True(isAllowed);
		}

		[Fact]
		public void WillReturnFalseOnAccountIfPermissionWasDeniedToUser()
		{
			permissionsBuilderService
				.Deny("/Account/Edit")
				.For(user)
				.On(account)
				.DefaultLevel()
				.Save();
			

			bool isAllowed = authorizationService.IsAllowed(user, account, "/Account/Edit");
			Assert.False(isAllowed);
		}

		[Fact]
		public void WillReturnTrueOnEntityGroupIfPermissionWasGrantedToUsersGroupAssociatedWithUser()
		{
			permissionsBuilderService
				.Allow("/Account/Edit")
				.For("Administrators")
				.On("Important Accounts")
				.DefaultLevel()
				.Save();
			

			bool isAllowed = authorizationService.IsAllowed(user, account, "/Account/Edit");
			Assert.True(isAllowed);
		}

		[Fact]
		public void WillReturnFalseOnAccountIfNoPermissionIsDefined()
		{
			bool isAllowed = authorizationService.IsAllowed(user, account, "/Account/Edit");
			Assert.False(isAllowed);
		}

		[Fact]
		public void WillReturnFalseOnAccountIfPermissionWasDeniedToUserOnTheGroupTheEntityIsAssociatedWith()
		{
			permissionsBuilderService
				.Deny("/Account/Edit")
				.For(user)
				.On("Important Accounts")
				.DefaultLevel()
				.Save();
			

			bool isAllowed = authorizationService.IsAllowed(user, account, "/Account/Edit");
			Assert.False(isAllowed);
		}

		[Fact]
		public void WillReturnTrueOnAccountIfPermissionWasAllowedToUserOnTheGroupTheEntityIsAssociatedWith()
		{
			permissionsBuilderService
				.Allow("/Account/Edit")
				.For(user)
				.On("Important Accounts")
				.DefaultLevel()
				.Save();
			

			bool isAllowed = authorizationService.IsAllowed(user, account, "/Account/Edit");
			Assert.True(isAllowed);
		}

        [Fact]
        public void WillReturnTrueOnGlobalIfPermissionWasAllowedOnGlobalButDeniedOnEntitiesGroup()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(user)
                .OnEverything()
                .DefaultLevel()
                .Save();
            

            permissionsBuilderService
                .Deny("/Account/Edit")
                .For(user)
                .On("Important Accounts")
                .DefaultLevel()
                .Save();
            

            bool IsAllowed = authorizationService.IsAllowed(user, "/Account/Edit");
            Assert.True(IsAllowed);
        }

        [Fact]
        public void WillReturnTrueOnGlobalIfPermissionWasAllowedOnGlobalButDeniedOnSpecificEntity()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(user)
                .OnEverything()
                .DefaultLevel()
                .Save();
            

            permissionsBuilderService
                .Deny("/Account/Edit")
                .For(user)
                .On(account)
                .DefaultLevel()
                .Save();
            

            bool IsAllowed = authorizationService.IsAllowed(user, "/Account/Edit");
            Assert.True(IsAllowed);
        }

		[Fact]
		public void UseSecondLevelCacheForSecurityQuestions_WillBeUpdatedWhenGoingThroughNHiberante()
		{
			permissionsBuilderService
				.Allow("/Account/Edit")
				.For(user)
				.On("Important Accounts")
				.DefaultLevel()
				.Save();
			

			bool isAllowed = authorizationService.IsAllowed(user, account, "/Account/Edit");
			Assert.True(isAllowed);

		    session.Delete("from Permission");
			

			// should return true since it loads from cache
			isAllowed = authorizationService.IsAllowed(user, account, "/Account/Edit");
			Assert.False(isAllowed);
		}


		[Fact]
		public void WillReturnFalseIfPermissionWasAllowedToChildGroupUserIsAssociatedWith()
		{
			authorizationRepository.CreateChildUserGroupOf("Administrators", "Helpdesk");
			

			permissionsBuilderService
			   .Allow("/Account/Edit")
			   .For("Helpdesk")
			   .On("Important Accounts")
			   .DefaultLevel()
			   .Save();
			

			bool isAllowed = authorizationService.IsAllowed(user, account, "/Account/Edit");
			Assert.False(isAllowed);
		}

		[Fact]
		public void WillReturnTrueIfPermissionWasAllowedToParentGroupUserIsAssociatedWith()
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
			

			bool isAllowed = authorizationService.IsAllowed(user, account, "/Account/Edit");
			Assert.True(isAllowed);
		}
	}
}
