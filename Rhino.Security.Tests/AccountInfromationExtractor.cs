namespace Rhino.Security.Tests
{
    using System;
    using Commons;
    using NHibernate.Expressions;

    public class AccountInfromationExtractor : IEntityInformationExtractor<Account>
    {
        private readonly IRepository<Account> accountsRepository;

        public AccountInfromationExtractor(IRepository<Account> account)
        {
            this.accountsRepository = account;
        }

        public Guid GetSecurityKeyFor(Account entity)
        {
            return entity.SecurityKey;
        }

        public string GetDescription(Guid securityKey)
        {
            Account account = accountsRepository.FindOne(Expression.Eq("SecurityKey", securityKey));
            return string.Format("Account: {0}", account.Name);
        }
    }
}