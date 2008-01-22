namespace Rhino.Security.Tests
{
    using Commons;
    using MbUnit.Framework;
    using NHibernate;

    [TestFixture]
    public class AuthorizationService_Queries_Fixture : DatabaseFixture
    {
        private ICriteria criteria;

        public override void SetUp()
        {
            base.SetUp();
            criteria = UnitOfWork.CurrentSession.CreateCriteria(typeof(Account),"account");
        }

        [Test]
        public void WillReturnNothingIfNoPermissionHasBeenDefined()
        {
            authorizationService.AddPermissionsToQuery(user, "/Account/Edit", criteria);
            Assert.IsEmpty(criteria.List());
        }

        [Test]
        public void WillReturnNothingIfOperationNotDefined()
        {
            authorizationService.AddPermissionsToQuery(user, "/Account/Delete", criteria);
            Assert.IsEmpty(criteria.List());
        }

        [Test]
        public void WillReturnTrueIfAllowPermissionWasDefined()
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
        public void WillReturnTrueIfAllowPermissionWasDefinedOnEverything()
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
    }
}