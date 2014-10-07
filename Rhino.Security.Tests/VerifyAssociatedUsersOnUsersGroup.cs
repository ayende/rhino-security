﻿using System;
using Microsoft.Practices.ServiceLocation;
using Rhino.Security.Interfaces;
using Xunit;
using Rhino.Security.Model;


namespace Rhino.Security.Tests
{
    public class VerifyAssociatedUsersOnUsersGroup : DatabaseFixture
    {
        protected Int64 idMarcus;
        protected Int64 idAyende;

        public VerifyAssociatedUsersOnUsersGroup()
        {
            var marcus = new Entities.User { Name = "marcus" };
            session.Save(marcus);
            session.Flush();
            session.Evict(marcus);

            var ayende = new Entities.User { Name = "ayende" };
            session.Save(ayende);
            session.Flush();
            session.Evict(ayende);

            idAyende = ayende.Id;
            idMarcus = marcus.Id;

            var fromDb = session.Get<Entities.User>(idAyende);
            Assert.NotNull(fromDb);
            Assert.Equal(ayende.Name, fromDb.Name);
            fromDb = session.Get<Entities.User>(idMarcus);
            Assert.NotNull(fromDb);
            Assert.Equal(marcus.Name, fromDb.Name);

            UsersGroup group = authorizationRepository.CreateUsersGroup("Admin");
            authorizationRepository.AssociateUserWith(ayende, "Admin");
            authorizationRepository.AssociateUserWith(marcus, "Admin");
            session.Flush();
            session.Evict(group);
        }

        [Fact]
        public void GetUsersByUsersGroup()
        {
            authorizationService = ServiceLocator.Current.GetInstance<IAuthorizationService>();
            permissionService = ServiceLocator.Current.GetInstance<IPermissionsService>();
            permissionsBuilderService = ServiceLocator.Current.GetInstance<IPermissionsBuilderService>();
            authorizationRepository = ServiceLocator.Current.GetInstance<IAuthorizationRepository>();

            var marcus = session.Get<Entities.User>(Convert.ToInt64(idMarcus));
            UsersGroup[] marcusGroups = authorizationRepository.GetAssociatedUsersGroupFor(marcus);
            Assert.Equal(1, marcusGroups.Length);
            Assert.Equal(2, marcusGroups[0].Users.Count);

            var ayende = session.Get<Entities.User>(Convert.ToInt64(idAyende));
            UsersGroup[] ayendeGroups = authorizationRepository.GetAssociatedUsersGroupFor(ayende);
            Assert.Equal(1, ayendeGroups.Length);
            Assert.Equal(2, ayendeGroups[0].Users.Count);

            Assert.Equal(2, authorizationRepository.GetUsersGroupByName("Admin").Users.Count);

            session.Dispose();
        }
    }
}
