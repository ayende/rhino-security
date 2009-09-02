using System;
using Microsoft.Practices.ServiceLocation;
using NHibernate;
using NHibernate.ByteCode.Castle;
using NHibernate.Cache;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Tool.hbm2ddl;
using Rhino.Security.Interfaces;
using Environment=NHibernate.Cfg.Environment;

namespace Rhino.Security.Tests
{

    public abstract class DatabaseFixture : IDisposable
	{
		protected Account account;
		protected IAuthorizationRepository authorizationRepository;
		protected IAuthorizationService authorizationService;
		protected IPermissionsBuilderService permissionsBuilderService;
		protected IPermissionsService permissionService;
		protected User user;

        protected ISession session;
        protected readonly ISessionFactory factory;

        protected DatabaseFixture()
		{
            BeforeSetup();

            SillyContainer.SessionProvider = (() => session);
            var sillyContainer = new SillyContainer();
            ServiceLocator.SetLocatorProvider(() => sillyContainer);

		    var cfg = new Configuration()
                .SetProperty(Environment.ConnectionDriver, typeof(SQLite20Driver).AssemblyQualifiedName)
                .SetProperty(Environment.Dialect, typeof(SQLiteDialect).AssemblyQualifiedName)
                .SetProperty(Environment.ConnectionString, ConnectionString)
                .SetProperty(Environment.ProxyFactoryFactoryClass, typeof(ProxyFactoryFactory).AssemblyQualifiedName)
                .SetProperty(Environment.ReleaseConnections, "on_close")
                .SetProperty(Environment.UseSecondLevelCache, "true")
                .SetProperty(Environment.UseQueryCache, "true")
                .SetProperty(Environment.CacheProvider,typeof(HashtableCacheProvider).AssemblyQualifiedName)
		        .AddAssembly(typeof (User).Assembly);

            Security.Configure<User>(cfg, SecurityTableStructure.Prefix);

            factory = cfg.BuildSessionFactory();

            session = factory.OpenSession();

            new SchemaExport(cfg).Execute(false, true, false, session.Connection, null);

            session.BeginTransaction();

            SetupEntities();
		}

        protected virtual void BeforeSetup()
        {
            
        }

        public virtual string ConnectionString
        {
            get { return "Data Source=:memory:"; }
        }

        public void Dispose()
		{
            if (session.Transaction.IsActive)
                session.Transaction.Rollback();
			session.Dispose();
		}

		private void SetupEntities()
		{
			user = new User {Name = "Ayende"};
		    account = new Account {Name = "south sand"};

		    session.Save(user);
			session.Save(account);

			authorizationService = ServiceLocator.Current.GetInstance<IAuthorizationService>();
            permissionService = ServiceLocator.Current.GetInstance<IPermissionsService>();
            permissionsBuilderService = ServiceLocator.Current.GetInstance<IPermissionsBuilderService>();
            authorizationRepository = ServiceLocator.Current.GetInstance<IAuthorizationRepository>();

			authorizationRepository.CreateUsersGroup("Administrators");
			authorizationRepository.CreateEntitiesGroup("Important Accounts");
			authorizationRepository.CreateOperation("/Account/Edit");


			authorizationRepository.AssociateUserWith(user, "Administrators");
			authorizationRepository.AssociateEntityWith(account, "Important Accounts");
		}
	}
}