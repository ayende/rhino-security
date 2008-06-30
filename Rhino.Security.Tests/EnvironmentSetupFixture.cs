using Rhino.Commons.ForTesting;
using Rhino.Security.Model;

namespace Rhino.Security.Tests
{
	using Commons;
	using MbUnit.Framework;

	[TestFixture]
	public class ActiveRecord_EnvironmentSetupFixture : EnvironmentSetupFixture
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
	public class NHibernate_EnvironmentSetupFixture : EnvironmentSetupFixture
	{
		public override PersistenceFramework PersistenceFramwork
		{
			get { return PersistenceFramework.NHibernate; }
		}

		public override string RhinoContainerConfig
		{
			get { return "nh-windsor.boo"; }
		}
	}

	public abstract class EnvironmentSetupFixture : DatabaseFixture
	{
		[Test]
		public void RepositoryIsNotProxied()
		{
			bool isProxy = IoC.Resolve<IRepository<Operation>>()
				.GetType().FullName.Contains("Proxy");
			Assert.IsFalse(isProxy);
		}
	}
}