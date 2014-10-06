using NHibernate;
using NHibernate.Criterion;
using Rhino.Security.Interfaces;

namespace Rhino.Security.Tests
{
    using System;

    public class AccountInformationExtractor : IEntityInformationExtractor<Entities.Account>
    {
        private readonly ISession session;

        public AccountInformationExtractor(ISession session)
        {
            this.session = session;
        }

        public Guid GetSecurityKeyFor(Entities.Account entity)
        {
            return entity.SecurityKey;
        }

        public string GetDescription(Guid securityKey)
        {
            var account = session.CreateCriteria<Entities.Account>()
                .Add(Restrictions.Eq("SecurityKey", securityKey))
                .UniqueResult<Entities.Account>();
            return string.Format("Account: {0}", account.Name);
        }

        /// <summary>
        /// Gets the name of the security key property.
        /// </summary>
        /// <value>The name of the security key property.</value>
        public string SecurityKeyPropertyName
        {
            get { return "SecurityKey"; }
        }
    }
}