using MeterReadings.Infrastructure.Entities;
using System.Collections.Immutable;

namespace MeterReadings.Infrastructure.Repositories
{
    public interface IAccountsRepository
    {
        public Task InsertAccounts(ImmutableList<Account> accounts, CancellationToken cancellationToken);
    }
}
