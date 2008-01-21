namespace Rhino.Security.Tests
{
    using System.IO;
    using Commons;
	using MbUnit.Framework;
	using Rhino.Commons.ForTesting;

	public class DatabaseFixture : TestFixtureBase
	{
		protected Account account;
		protected IAuthorizationEditingService authorizationEditingService;
		protected IAuthorizationService authorizationService;
		protected IPermissionsBuilderService permissionsBuilderService;
		protected IPermissionsService permissionService;
		protected User user;

		private void SetupEntities()
		{
			user = new User();
			user.Name = "Ayende";
			account = new Account();
			account.Name = "south sand";

			UnitOfWork.CurrentSession.Save(user);
			UnitOfWork.CurrentSession.Save(account);

			authorizationService = IoC.Resolve<IAuthorizationService>();
			permissionService = IoC.Resolve<IPermissionsService>();
			permissionsBuilderService = IoC.Resolve<IPermissionsBuilderService>();
			authorizationEditingService = IoC.Resolve<IAuthorizationEditingService>();
			authorizationEditingService.CreateUsersGroup("Administrators");
			authorizationEditingService.CreateEntitiesGroup("Important Accounts");
			authorizationEditingService.CreateOperation("/Account/Edit");

			UnitOfWork.Current.TransactionalFlush();

			authorizationEditingService.AssociateUserWith(user, "Administrators");
			authorizationEditingService.AssociateEntityWith(account, "Important Accounts");

			UnitOfWork.Current.TransactionalFlush();
		}

		[SetUp]
		public virtual void SetUp()
		{
			Security.UseSecuritySchema = false;
			Security.PrepareForActiveRecordInitialization<User>();
			MappingInfo from = MappingInfo.From(typeof (IUser).Assembly, typeof (User).Assembly);
			FixtureInitialize(PersistenceFramework.ActiveRecord, "windsor.boo", DatabaseEngine.MsSqlCe, from);
			CurrentContext.CreateUnitOfWork();

			SetupEntities();
		}

		[TearDown]
		public void TearDown()
		{
			CurrentContext.DisposeUnitOfWork();
            try
            {
                File.Copy("TempDB.sdf", "CopyTempDB.dsf");
            }
            catch
            {
                // we ignore this, because we are doing this to allow us
                // to lock the copy of the temp db without breaking all the tests
                // this make it easier to jump between the DB and the tests
            }
		}
	}
}