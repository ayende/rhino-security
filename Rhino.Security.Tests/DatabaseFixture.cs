using Rhino.Security.ActiveRecord;
using Rhino.Security.Interfaces;

namespace Rhino.Security.Tests
{
	using Commons;
	using MbUnit.Framework;
	using Rhino.Commons.ForTesting;

	public abstract class DatabaseFixture : DatabaseTestFixtureBase
	{
		protected Account account;
		protected IAuthorizationRepository authorizationRepository;
		protected IAuthorizationService authorizationService;
		protected IPermissionsBuilderService permissionsBuilderService;
		protected IPermissionsService permissionService;
		protected User user;

		[SetUp]
		public virtual void SetUp()
		{
			MappingInfo from = MappingInfo.From(
				typeof(IUser).Assembly,
				typeof(User).Assembly,
				typeof(RegisterRhinoSecurityMappingAttribute).Assembly);
			IntializeNHibernateAndIoC(PersistenceFramwork, RhinoContainerConfig, GetDatabaseEngine(), from);
			CurrentContext.CreateUnitOfWork();

			SetupEntities();
		}

		public abstract PersistenceFramework PersistenceFramwork
		{
			get;
		}

		public abstract string RhinoContainerConfig { get; }

		protected virtual DatabaseEngine GetDatabaseEngine()
		{
			return DatabaseEngine.SQLite;
		}

		[TearDown]
		public void TearDown()
		{
			CurrentContext.DisposeUnitOfWork();
		}

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
			authorizationRepository = IoC.Resolve<IAuthorizationRepository>();
			authorizationRepository.CreateUsersGroup("Administrators");
			authorizationRepository.CreateEntitiesGroup("Important Accounts");
			authorizationRepository.CreateOperation("/Account/Edit");

			UnitOfWork.Current.TransactionalFlush();

			authorizationRepository.AssociateUserWith(user, "Administrators");
			authorizationRepository.AssociateEntityWith(account, "Important Accounts");

			UnitOfWork.Current.TransactionalFlush();
		}
	}
}