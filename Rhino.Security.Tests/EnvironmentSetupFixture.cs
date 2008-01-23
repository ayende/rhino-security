namespace Rhino.Security.Tests
{
	using Commons;
	using MbUnit.Framework;

	[TestFixture]
	public class EnvironmentSetupFixture : DatabaseFixture
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