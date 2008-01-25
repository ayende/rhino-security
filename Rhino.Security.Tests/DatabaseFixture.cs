namespace Rhino.Security.Tests
{
    using System.IO;
    using Commons;
	using MbUnit.Framework;
    using NHibernate;
    using NHibernate.Cache;
    using NHibernate.Cfg;
    using Rhino.Commons.ForTesting;

	public class DatabaseFixture : TestFixtureBase
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
			Security.PrepareForActiveRecordInitialization<User>(SecurityTableStructure.Prefix);
			MappingInfo from = MappingInfo.From(typeof (IUser).Assembly, typeof (User).Assembly);
			FixtureInitialize(PersistenceFramework.ActiveRecord, "windsor.boo", GetDatabaseEngine(), from);
			CurrentContext.CreateUnitOfWork();

			SetupEntities();
		}

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

	public class EnableTestCaching : INHibernateInitializationAware
	{
		public void Configured(Configuration cfg)
		{
			cfg.Properties[Environment.UseQueryCache] = "true";
			cfg.Properties[Environment.UseSecondLevelCache] = "true";
			cfg.Properties[Environment.CacheProvider] = typeof (HashtableCacheProvider).AssemblyQualifiedName;
		}

		public void Initialized(Configuration cfg, ISessionFactory sessionFactory)
		{
		}
	}
}