namespace Rhino.Security.Tests
{
    using MbUnit.Framework;
    using Rhino.Commons.ForTesting;

    public class DatabaseFixture : TestFixtureBase
    {
        [SetUp]
        public virtual void SetUp()
        {
            Security.UseSecuritySchema = false;
            Security.PrepareForActiveRecordInitialization<User>();
            MappingInfo from = MappingInfo.From(typeof(IUser).Assembly, typeof(User).Assembly);
            FixtureInitialize(PersistenceFramework.ActiveRecord, "windsor.boo", DatabaseEngine.MsSqlCe, from);
            CurrentContext.CreateUnitOfWork();
        }

        [TearDown]
        public void TearDown()
        {
            CurrentContext.DisposeUnitOfWork();
        }
    }
}