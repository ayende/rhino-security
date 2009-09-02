using NHibernate;
using NHibernate.Criterion;
using Rhino.Security.Interfaces;

namespace Rhino.Security.Tests
{
    using System;

    public class AccountInfromationExtractor : IEntityInformationExtractor<Account>
    {
        private readonly ISession session;

        public AccountInfromationExtractor(ISession session)
        {
            this.session = session;
        }

        public Guid GetSecurityKeyFor(Account entity)
        {
            return entity.SecurityKey;
        }

        public string GetDescription(Guid securityKey)
        {
            Account account = session.CreateCriteria<Account>()
                .Add(Restrictions.Eq("SecurityKey", securityKey))
                .UniqueResult<Account>();
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