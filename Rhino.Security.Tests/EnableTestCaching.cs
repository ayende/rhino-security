namespace Rhino.Security.Tests
{
    using Commons;
    using NHibernate;
    using NHibernate.Cache;
    using NHibernate.Cfg;

    public class EnableTestCaching : INHibernateInitializationAware
    {
    	public void BeforeInitialization()
    	{
    		
    	}

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