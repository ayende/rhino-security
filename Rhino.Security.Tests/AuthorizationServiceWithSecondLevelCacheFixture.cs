using System;
using System.IO;
using Microsoft.Practices.ServiceLocation;
using Rhino.Security.Interfaces;
using Rhino.Security.Model;
using Xunit;

namespace Rhino.Security.Tests
{
    public class AuthorizationServiceWithSecondLevelCacheFixture : DatabaseFixture
    {
        // we need those to ensure that we aren't leaving the 2nd level
        // cache in an inconsistent state after deletion
        //TODO: Add entity to group, save, remove and query
        //TODO: Add user to group, save, remove and query
        //TODO: Add nested users group save, remove and query

        public override string ConnectionString
        {
            get
            {
                return "Data Source=test.db";
            }
        }

        protected override void BeforeSetup()
        {
            if (File.Exists("test.db"))
                File.Delete("test.db");
        }

        [Fact]
        public void UseSecondLevelCacheForSecurityQuestions()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(user)
                .On("Important Accounts")
                .DefaultLevel()
                .Save();

            session.Flush();
            session.Transaction.Commit();
            session.Dispose();

            using (var s2 = factory.OpenSession())// load into the 2nd level cache
            using (s2.BeginTransaction())
            {
                SillyContainer.SessionProvider = () => s2;
                var anotherAuthorizationService = ServiceLocator.Current.GetInstance<IAuthorizationService>();
                Assert.True(anotherAuthorizationService.IsAllowed(user, account, "/Account/Edit"));

                s2.Transaction.Commit();
            }

            using (var s2 = factory.OpenSession())//remove the data from the cache
            using (s2.BeginTransaction())
            {
                using (var command = s2.Connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM security_Permissions";
                    command.ExecuteNonQuery();
                }

                s2.Transaction.Commit();
            }
            using (var s3 = factory.OpenSession())
            using (s3.BeginTransaction())
            {
                // should return true since it loads from cache
                SillyContainer.SessionProvider = () => s3;
                var anotherAuthorizationService = ServiceLocator.Current.GetInstance<IAuthorizationService>();
                Assert.True(anotherAuthorizationService.IsAllowed(user, account, "/Account/Edit"));

                s3.Transaction.Commit();
            }
        }

        [Fact]
        public void RemovingUserFromGroupInvalidatesSecondLevelCache()
        {
            var users = authorizationRepository.CreateUsersGroup("Users");
            authorizationRepository.AssociateUserWith(user, users);

            session.Flush();
            session.Transaction.Commit();
            session.Dispose();
            
            using (var s1 = factory.OpenSession())
            using (var tx = s1.BeginTransaction())
            {
                SillyContainer.SessionProvider = () => s1;
                var anotherAuthorizationRepository = ServiceLocator.Current.GetInstance<IAuthorizationRepository>();
                UsersGroup[] initialGroups = anotherAuthorizationRepository.GetAssociatedUsersGroupFor(user);
                Assert.Equal(2, initialGroups.Length);
                Assert.Equal("Administrators", initialGroups[0].Name);
                Assert.Equal("Users", initialGroups[1].Name);
                tx.Commit();
            }

            using (var s2 = factory.OpenSession())
            using (var tx = s2.BeginTransaction())
            {
                SillyContainer.SessionProvider = () => s2;
                var anotherAuthorizationRepository = ServiceLocator.Current.GetInstance<IAuthorizationRepository>();
                anotherAuthorizationRepository.DetachUserFromGroup(user, "Users");
                tx.Commit();
            }

            using (var s3 = factory.OpenSession())
            using (var tx = s3.BeginTransaction())
            {
                SillyContainer.SessionProvider = () => s3;
                var anotherAuthorizationRepository = ServiceLocator.Current.GetInstance<IAuthorizationRepository>();
                UsersGroup[] newGroups = anotherAuthorizationRepository.GetAssociatedUsersGroupFor(user);
                Assert.Equal(1, newGroups.Length);
                Assert.Equal("Administrators", newGroups[0].Name);
                tx.Commit();
            }
        }
    }
}