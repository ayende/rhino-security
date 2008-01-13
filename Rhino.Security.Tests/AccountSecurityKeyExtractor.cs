namespace Rhino.Security.Tests
{
    using System;

    public class AccountSecurityKeyExtractor : IEntitySecurityKeyExtractor<Account>
    {
        public Guid GetSecurityKeyFor(Account entity)
        {
            return entity.SecurityKey;
        }
    }
}