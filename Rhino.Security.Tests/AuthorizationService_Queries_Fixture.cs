using Rhino.Commons.ForTesting;
using Rhino.Security.Model;

namespace Rhino.Security.Tests
{
    using Commons;
    using MbUnit.Framework;
    using NHibernate;
    using NHibernate.Criterion;

	[TestFixture]
	public class ActiveRecord_AuthorizationService_Queries_Fixture : AuthorizationService_Queries_Fixture
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
	public class NHibernate_AuthorizationService_Queries_Fixture : AuthorizationService_Queries_Fixture
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

    public abstract class AuthorizationService_Queries_Fixture : DatabaseFixture
    {
        private ICriteria criteria;

        public override void SetUp()
        {
            base.SetUp();
            criteria = UnitOfWork.CurrentSession.CreateCriteria(typeof(Account), "account");
        }

        [Test]
        public void WillReturnNothingIfNoPermissionHasBeenDefined()
        {
            authorizationService.AddPermissionsToQuery(user, "/Account/Edit", criteria);
            Assert.IsEmpty(criteria.List());
        }

        [Test]
        public void WillReturnNothingForUsersGroupIfNoPermissionHasBeenDefined()
        {
            UsersGroup[] usersgroups = authorizationRepository.GetAssociatedUsersGroupFor(user);
            authorizationService.AddPermissionsToQuery(usersgroups[0], "/Account/Edit", criteria);
            Assert.IsEmpty(criteria.List());
        }

        [Test]
        public void WillReturnNothingIfOperationNotDefined()
        {
            authorizationService.AddPermissionsToQuery(user, "/Account/Delete", criteria);
            Assert.IsEmpty(criteria.List());
        }

        [Test]
        public void WillReturnNothingForUsersGroupIfOperationNotDefined()
        {
            UsersGroup[] usersgroups = authorizationRepository.GetAssociatedUsersGroupFor(user);
            authorizationService.AddPermissionsToQuery(usersgroups[0], "/Account/Delete", criteria);
            Assert.IsEmpty(criteria.List());
        }

        [Test]
        public void WillReturnResultIfAllowPermissionWasDefined()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(user)
                .On(account)
                .DefaultLevel()
                .Save();
            UnitOfWork.Current.TransactionalFlush();

            authorizationService.AddPermissionsToQuery(user, "/Account/Edit", criteria);
            Assert.IsNotEmpty(criteria.List());
        }

        [Test]
        public void WillReturnResultForUsersGroupIfAllowPermissionWasDefined()
        {
            UsersGroup usersgroup = authorizationRepository.GetUsersGroupByName("Administrators");
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(usersgroup)
                .On(account)
                .DefaultLevel()
                .Save();
            UnitOfWork.Current.TransactionalFlush();

            authorizationService.AddPermissionsToQuery(usersgroup, "/Account/Edit", criteria);
            Assert.IsNotEmpty(criteria.List());
        }

        [Test]
        public void WillReturnResultIfAllowPermissionWasDefinedOnEverything()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(user)
                .OnEverything()
                .DefaultLevel()
                .Save();
            UnitOfWork.Current.TransactionalFlush();

            authorizationService.AddPermissionsToQuery(user, "/Account/Edit", criteria);
            Assert.IsNotEmpty(criteria.List());
        }

        [Test]
        public void WillReturnResultForUsersGroupIfAllowPermissionWasDefinedOnEverything()
        {
            UsersGroup usersgroup = authorizationRepository.GetUsersGroupByName("Administrators");
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(usersgroup)
                .OnEverything()
                .DefaultLevel()
                .Save();
            UnitOfWork.Current.TransactionalFlush();

            authorizationService.AddPermissionsToQuery(usersgroup, "/Account/Edit", criteria);
            Assert.IsNotEmpty(criteria.List());
        }

        [Test]
        public void WillReturnNothingIfAllowPermissionWasDefinedOnGroupAndDenyPermissionOnUser()
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
            UnitOfWork.Current.TransactionalFlush();

            authorizationService.AddPermissionsToQuery(user, "/Account/Edit", criteria);
            Assert.IsEmpty(criteria.List());

        }

        [Test]
        public void WillReturnResultForUsersGroupIfAllowPermissionWasDefinedOnGroupAndDenyPermissionOnUser()
        {
            UsersGroup usersgroup = authorizationRepository.GetUsersGroupByName("Administrators");
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For("Administrators")
                .OnEverything()
                .DefaultLevel()
                .Save();
            permissionsBuilderService
                .Deny("/Account/Edit")
                .For(user)
                .OnEverything()
                .DefaultLevel()
                .Save();
            UnitOfWork.Current.TransactionalFlush();

            authorizationService.AddPermissionsToQuery(usersgroup, "/Account/Edit", criteria);
            Assert.IsNotEmpty(criteria.List());

        }


        [Test]
        public void WillReturnNothingIfAllowedPermissionWasDefinedWithDenyPermissionWithHigherLevel()
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
            UnitOfWork.Current.TransactionalFlush();

            authorizationService.AddPermissionsToQuery(user, "/Account/Edit", criteria);
            Assert.IsEmpty(criteria.List());
        }

        [Test]
        public void WillReturnNothingForUsersGroupIfAllowedPermissionWasDefinedWithDenyPermissionWithHigherLevel()
        {
            UsersGroup usersgroup = authorizationRepository.GetUsersGroupByName("Administrators");
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(usersgroup)
                .OnEverything()
                .DefaultLevel()
                .Save();
            permissionsBuilderService
                .Deny("/Account/Edit")
                .For(usersgroup)
                .OnEverything()
                .Level(5)
                .Save();
            UnitOfWork.Current.TransactionalFlush();

            authorizationService.AddPermissionsToQuery(usersgroup, "/Account/Edit", criteria);
            Assert.IsEmpty(criteria.List());
        }

        [Test]
        public void WillReturnResultIfAllowedPermissionWasDefinedWithDenyPermissionWithLowerLevel()
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
            UnitOfWork.Current.TransactionalFlush();

            authorizationService.AddPermissionsToQuery(user, "/Account/Edit", criteria);
            Assert.IsNotEmpty(criteria.List());
        }

        [Test]
        public void WillReturnResultForUsersGroupIfAllowedPermissionWasDefinedWithDenyPermissionWithLowerLevel()
        {
            UsersGroup usersgroup = authorizationRepository.GetUsersGroupByName("Administrators");
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(usersgroup)
                .OnEverything()
                .Level(10)
                .Save();
            permissionsBuilderService
                .Deny("/Account/Edit")
                .For(usersgroup)
                .OnEverything()
                .Level(5)
                .Save();
            UnitOfWork.Current.TransactionalFlush();

            authorizationService.AddPermissionsToQuery(usersgroup, "/Account/Edit", criteria);
            Assert.IsNotEmpty(criteria.List());
        }

        [Test]
        public void WillReturnResultOnAccountIfPermissionWasGrantedOnAnything()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(user)
                .OnEverything()
                .DefaultLevel()
                .Save();
            UnitOfWork.Current.TransactionalFlush();


            authorizationService.AddPermissionsToQuery(user, "/Account/Edit", criteria);
            Assert.IsNotEmpty(criteria.List());
        }

        [Test]
        public void WillReturnResultForUsersGroupOnAccountIfPermissionWasGrantedOnAnything()
        {
            UsersGroup usersgroup = authorizationRepository.GetUsersGroupByName("Administrators");
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(usersgroup)
                .OnEverything()
                .DefaultLevel()
                .Save();
            UnitOfWork.Current.TransactionalFlush();


            authorizationService.AddPermissionsToQuery(usersgroup, "/Account/Edit", criteria);
            Assert.IsNotEmpty(criteria.List());
        }

        [Test]
        public void WillReturNothingOnAccountIfPermissionWasDeniedOnAnything()
        {
            permissionsBuilderService
                .Deny("/Account/Edit")
                .For(user)
                .OnEverything()
                .DefaultLevel()
                .Save();
            UnitOfWork.Current.TransactionalFlush();

            authorizationService.AddPermissionsToQuery(user, "/Account/Edit", criteria);
            Assert.IsEmpty(criteria.List());
        }

        [Test]
        public void WillReturNothingForUsersGroupOnAccountIfPermissionWasDeniedOnAnything()
        {
            UsersGroup usersgroup = authorizationRepository.GetUsersGroupByName("Administrators");
            permissionsBuilderService
                .Deny("/Account/Edit")
                .For(usersgroup)
                .OnEverything()
                .DefaultLevel()
                .Save();
            UnitOfWork.Current.TransactionalFlush();

            authorizationService.AddPermissionsToQuery(usersgroup, "/Account/Edit", criteria);
            Assert.IsEmpty(criteria.List());
        }

        [Test]
        public void WillReturnResultOnAccountIfPermissionWasGrantedOnGroupAssociatedWithUser()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For("Administrators")
                .On(account)
                .DefaultLevel()
                .Save();
            UnitOfWork.Current.TransactionalFlush();

            authorizationService.AddPermissionsToQuery(user, "/Account/Edit", criteria);
            Assert.IsNotEmpty(criteria.List());
        }

        [Test]
        public void WillReturnResultForUsersGroupOnAccountIfPermissionWasGrantedOnGroup()
        {
            UsersGroup usersgroup = authorizationRepository.GetUsersGroupByName("Administrators");
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(usersgroup)
                .On(account)
                .DefaultLevel()
                .Save();
            UnitOfWork.Current.TransactionalFlush();

            authorizationService.AddPermissionsToQuery(usersgroup, "/Account/Edit", criteria);
            Assert.IsNotEmpty(criteria.List());
        }


        [Test]
        public void WillReturnNothingOnAccountIfPermissionWasDeniedOnGroupAssociatedWithUser()
        {
            permissionsBuilderService
                .Deny("/Account/Edit")
                .For("Administrators")
                .On(account)
                .DefaultLevel()
                .Save();
            UnitOfWork.Current.TransactionalFlush();

            authorizationService.AddPermissionsToQuery(user, "/Account/Edit", criteria);
            Assert.IsEmpty(criteria.List());
        }

        [Test]
        public void WillReturnNothingForUsersGroupoOnAccountIfPermissionWasDeniedOnGroup()
        {
            UsersGroup usersgroup = authorizationRepository.GetUsersGroupByName("Administrators");
            permissionsBuilderService
                .Deny("/Account/Edit")
                .For(usersgroup)
                .On(account)
                .DefaultLevel()
                .Save();
            UnitOfWork.Current.TransactionalFlush();

            authorizationService.AddPermissionsToQuery(usersgroup, "/Account/Edit", criteria);
            Assert.IsEmpty(criteria.List());
        }

        [Test]
        public void WillReturnResultOnAccountIfPermissionWasGrantedToUser()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(user)
                .On(account)
                .DefaultLevel()
                .Save();
            UnitOfWork.Current.TransactionalFlush();

            authorizationService.AddPermissionsToQuery(user, "/Account/Edit", criteria);
            Assert.IsNotEmpty(criteria.List());
        }

        [Test]
        public void WillReturnNothingOnAccountIfPermissionWasDeniedToUser()
        {
            permissionsBuilderService
                .Deny("/Account/Edit")
                .For(user)
                .On(account)
                .DefaultLevel()
                .Save();
            UnitOfWork.Current.TransactionalFlush();

            authorizationService.AddPermissionsToQuery(user, "/Account/Edit", criteria);
            Assert.IsEmpty(criteria.List());
        }

        [Test]
        public void WillReturnResultOnEntityGroupIfPermissionWasGrantedToUsersGroupAssociatedWithUser()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For("Administrators")
                .On("Important Accounts")
                .DefaultLevel()
                .Save();
            UnitOfWork.Current.TransactionalFlush();

            authorizationService.AddPermissionsToQuery(user, "/Account/Edit", criteria);
            Assert.IsNotEmpty(criteria.List());
        }

        [Test]
        public void WillReturnResultForUsersGroupOnEntityGroupIfPermissionWasGrantedToUsersGroup()
        {
            UsersGroup usersgroup = authorizationRepository.GetUsersGroupByName("Administrators");
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(usersgroup)
                .On("Important Accounts")
                .DefaultLevel()
                .Save();
            UnitOfWork.Current.TransactionalFlush();

            authorizationService.AddPermissionsToQuery(usersgroup, "/Account/Edit", criteria);
            Assert.IsNotEmpty(criteria.List());
        }

        [Test]
        public void WillReturnNothingOnAccountIfPermissionWasDeniedToUserOnTheGroupTheEntityIsAssociatedWith()
        {
            permissionsBuilderService
                .Deny("/Account/Edit")
                .For(user)
                .On("Important Accounts")
                .DefaultLevel()
                .Save();
            UnitOfWork.Current.TransactionalFlush();

            authorizationService.AddPermissionsToQuery(user, "/Account/Edit", criteria);
            Assert.IsEmpty(criteria.List());
        }

        [Test]
        public void WillReturnNothingForUsersGroupOnAccountIfPermissionWasDeniedToUserOnTheGroupTheEntityIsAssociatedWith()
        {
            UsersGroup usersgroup = authorizationRepository.GetUsersGroupByName("Administrators");
            permissionsBuilderService
                .Deny("/Account/Edit")
                .For(usersgroup)
                .On("Important Accounts")
                .DefaultLevel()
                .Save();
            UnitOfWork.Current.TransactionalFlush();

            authorizationService.AddPermissionsToQuery(usersgroup, "/Account/Edit", criteria);
            Assert.IsEmpty(criteria.List());
        }

        [Test]
        public void WillReturnResultOnAccountIfPermissionWasAllowedToUserOnTheGroupTheEntityIsAssociatedWith()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(user)
                .On("Important Accounts")
                .DefaultLevel()
                .Save();
            UnitOfWork.Current.TransactionalFlush();

            authorizationService.AddPermissionsToQuery(user, "/Account/Edit", criteria);
            Assert.IsNotEmpty(criteria.List());
        }

        [Test]
        public void WillReturnResultForUsersGroupOnAccountIfPermissionWasAllowedToUserOnTheGroupTheEntityIsAssociatedWith()
        {
            UsersGroup usersgroup = authorizationRepository.GetUsersGroupByName("Administrators");
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(usersgroup)
                .On("Important Accounts")
                .DefaultLevel()
                .Save();
            UnitOfWork.Current.TransactionalFlush();

            authorizationService.AddPermissionsToQuery(usersgroup, "/Account/Edit", criteria);
            Assert.IsNotEmpty(criteria.List());
        }

        [Test]
        public void WillReturnNothingIfPermissionWasAllowedToChildGroupUserIsAssociatedWith()
        {
            authorizationRepository.CreateChildUserGroupOf("Administrators", "Helpdesk");
            UnitOfWork.Current.TransactionalFlush();

            permissionsBuilderService
               .Allow("/Account/Edit")
               .For("Helpdesk")
               .On("Important Accounts")
               .DefaultLevel()
               .Save();
            UnitOfWork.Current.TransactionalFlush();

            authorizationService.AddPermissionsToQuery(user, "/Account/Edit", criteria);
            Assert.IsEmpty(criteria.List());
        }

        [Test]
        public void WillReturnNothingForUsersGroupIfPermissionWasAllowedToChildGroupOfVerifiedUsersGroup()
        {
            UsersGroup usersgroup = authorizationRepository.GetUsersGroupByName("Administrators");
            authorizationRepository.CreateChildUserGroupOf("Administrators", "Helpdesk");
            UnitOfWork.Current.TransactionalFlush();

            permissionsBuilderService
               .Allow("/Account/Edit")
               .For("Helpdesk")
               .On("Important Accounts")
               .DefaultLevel()
               .Save();
            UnitOfWork.Current.TransactionalFlush();

            authorizationService.AddPermissionsToQuery(usersgroup, "/Account/Edit", criteria);
            Assert.IsEmpty(criteria.List());
        }

        [Test]
        public void WillReturnResultIfPermissionWasAllowedToParentGroupUserIsAssociatedWith()
        {
            authorizationRepository.CreateChildUserGroupOf("Administrators", "Helpdesk");
            UnitOfWork.Current.TransactionalFlush();

            authorizationRepository.DetachUserFromGroup(user, "Administrators");
            authorizationRepository.AssociateUserWith(user, "Helpdesk");
            UnitOfWork.Current.TransactionalFlush();

            permissionsBuilderService
               .Allow("/Account/Edit")
               .For("Administrators")
               .On("Important Accounts")
               .DefaultLevel()
               .Save();
            UnitOfWork.Current.TransactionalFlush();

            authorizationService.AddPermissionsToQuery(user, "/Account/Edit", criteria);
            Assert.IsNotEmpty(criteria.List());
        }

        [Test]
        public void WillReturnNothingForUsersGroupIfPermissionWasAllowedToParentGroupOfVerifiedUsersGroup()
        {            
            authorizationRepository.CreateChildUserGroupOf("Administrators", "Helpdesk");
            UsersGroup usersgroup = authorizationRepository.GetUsersGroupByName("Helpdesk");
            UnitOfWork.Current.TransactionalFlush();
            
            UnitOfWork.Current.TransactionalFlush();

            permissionsBuilderService
               .Allow("/Account/Edit")
               .For("Administrators")
               .On("Important Accounts")
               .DefaultLevel()
               .Save();
            UnitOfWork.Current.TransactionalFlush();

            authorizationService.AddPermissionsToQuery(usersgroup, "/Account/Edit", criteria);
            Assert.IsEmpty(criteria.List());
        }

        [Test]
        public void WillReturnNothingIfOperationNotDefined_WithDetachedCriteria()
        {
            DetachedCriteria detachedCriteria = DetachedCriteria.For<Account>();
            authorizationService.AddPermissionsToQuery(user, "/Account/Delete", detachedCriteria);
            Assert.IsEmpty(detachedCriteria.GetExecutableCriteria(UnitOfWork.CurrentSession).List());
        }

        [Test]
        public void WillReturnNothingForUsersGroupIfOperationNotDefined_WithDetachedCriteria()
        {
            UsersGroup usersgroup = authorizationRepository.GetUsersGroupByName("Administrators");
            DetachedCriteria detachedCriteria = DetachedCriteria.For<Account>();
            authorizationService.AddPermissionsToQuery(usersgroup, "/Account/Delete", detachedCriteria);
            Assert.IsEmpty(detachedCriteria.GetExecutableCriteria(UnitOfWork.CurrentSession).List());
        }

        [Test]
        public void WillReturnResultIfAllowPermissionWasDefined_WithDetachedCriteria_AndConditions()
        {
            DetachedCriteria detachedCriteria = DetachedCriteria.For<Account>()
                .Add(Expression.Like("Name", "South",MatchMode.Start))
                ;
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(user)
                .On(account)
                .DefaultLevel()
                .Save();
            UnitOfWork.Current.TransactionalFlush();

            authorizationService.AddPermissionsToQuery(user, "/Account/Edit", detachedCriteria);
            Assert.IsNotEmpty(detachedCriteria.GetExecutableCriteria(UnitOfWork.CurrentSession).List());
        }

        [Test]
        public void WillReturnResultForUsersGroupIfAllowPermissionWasDefined_WithDetachedCriteria_AndConditions()
        {
            UsersGroup usersgroup = authorizationRepository.GetUsersGroupByName("Administrators");
            DetachedCriteria detachedCriteria = DetachedCriteria.For<Account>()
                .Add(Expression.Like("Name", "South", MatchMode.Start))
                ;
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(usersgroup)
                .On(account)
                .DefaultLevel()
                .Save();
            UnitOfWork.Current.TransactionalFlush();

            authorizationService.AddPermissionsToQuery(usersgroup, "/Account/Edit", detachedCriteria);
            Assert.IsNotEmpty(detachedCriteria.GetExecutableCriteria(UnitOfWork.CurrentSession).List());
        }
    }
}