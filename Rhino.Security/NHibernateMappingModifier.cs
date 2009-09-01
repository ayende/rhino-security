using System;
using NHibernate;
using NHibernate.Cfg;
using Rhino.Security.Impl.MappingRewriting;

namespace Rhino.Security
{
	/// <summary>
	/// Modify the NHibernate configuration to match the Rhino Security requirements
	/// </summary>
	public class NHibernateMappingModifier : INHibernateInitializationAware
	{
		private readonly SecurityTableStructure securityTableStructure;
		private readonly Type userType;

		/// <summary>
		/// Initializes a new instance of the <see cref="NHibernateMappingModifier"/> class.
		/// </summary>
		/// <param name="structure">The structure.</param>
		/// <param name="userType">Name of the user entity.</param>
		public NHibernateMappingModifier(SecurityTableStructure structure, Type userType)
		{
			securityTableStructure = structure;
			this.userType = userType;
		}

		/// <summary>
		/// not used
		/// </summary>
		public void BeforeInitialization()
		{
			
		}

		/// <summary>
		/// Configure the specified configuration to match Rhino Security requirements
		/// </summary>
		public void Configured(Configuration cfg)
		{
			if (cfg.Properties.ContainsKey("rhino.security.skipregistration")) return;

			new SchemaChanger(cfg, securityTableStructure).Change();
			new UserMapper(cfg, userType).Map();
		}

		/// <summary>
		/// Not used
		/// </summary>
		public void Initialized(Configuration cfg, ISessionFactory sessionFactory)
		{
		}
	}
}
