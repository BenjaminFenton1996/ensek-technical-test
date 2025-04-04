using MeterReadings.Infrastructure.Entities;
using System.Collections.Immutable;

namespace MeterReadings.Infrastructure.Repositories
{
    internal class AccountsRepository : IAccountsRepository
    {
        private readonly EnergyCompanyDbContext _context;
        public AccountsRepository(EnergyCompanyDbContext context)
        {
            _context = context;
        }

        public async Task InsertAccounts(ImmutableList<Account> accounts, CancellationToken cancellationToken = default)
        {
            await _context.Accounts.AddRangeAsync(accounts, cancellationToken);
        }
    }
}
