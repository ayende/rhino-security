using System;
using System.Collections.Generic;
using Microsoft.Practices.ServiceLocation;
using NHibernate;
using Rhino.Security.Interfaces;
using Rhino.Security.Services;

namespace Rhino.Security.Tests
{
    public class SillyContainer : ServiceLocatorImplBase
    {
        public static Func<ISession> SessionProvider;


        protected override object DoGetInstance(Type serviceType, string key)
        {
            if (serviceType == typeof(IAuthorizationService))
                return new AuthorizationService(GetInstance<IPermissionsService>(),
                                                GetInstance<IAuthorizationRepository>());
            if (serviceType == typeof(IAuthorizationRepository))
                return new AuthorizationRepository(SessionProvider());
            if (serviceType == typeof(IPermissionsBuilderService))
                return new PermissionsBuilderService(SessionProvider(), GetInstance<IAuthorizationRepository>());
            if (serviceType == typeof(IPermissionsService))
                return new PermissionsService(GetInstance<IAuthorizationRepository>(), SessionProvider());
            if (serviceType == typeof(IEntityInformationExtractor<Account>))
                return new AccountInfromationExtractor(SessionProvider());
            throw new NotImplementedException();
        }

        protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
        {
            throw new NotImplementedException();
        }
    }
}