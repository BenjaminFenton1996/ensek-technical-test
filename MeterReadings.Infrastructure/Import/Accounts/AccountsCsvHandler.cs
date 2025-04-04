using CsvHelper;
using MeterReadings.Infrastructure.Entities;
using MeterReadings.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;

namespace MeterReadings.Infrastructure.Import.Accounts
{
    public class AccountsCsvHandler : ICsvHandler
    {
        private readonly IAccountsRepository _accountsRepository;
        private readonly ILogger<AccountsCsvHandler> _logger;

        public AccountsCsvHandler(IAccountsRepository accountsRepository, ILogger<AccountsCsvHandler> logger)
        {
            _accountsRepository = accountsRepository;
            _logger = logger;
        }

        public bool CanParse(ImmutableArray<string> headers)
        {
            if (headers is [
                AccountCsvValidColumns.AccountIdHeader,
                AccountCsvValidColumns.FirstNameHeader,
                AccountCsvValidColumns.LastNameHeader
                ])
            {
                return true;
            }
            return false;
        }

        public async Task<ImportResult> ImportAsync(CsvReader csv, CancellationToken cancellationToken = default)
        {
            var (parsedRows, totalRows) = await CsvImportParser<AccountImportRow>.ReadCsvAsync(csv, AccountsCsvRowParser.ParseRow);
            var accounts = new List<Account>();
            foreach (var row in parsedRows)
            {
                var account = new Account
                {
                    AccountId = row.AccountId,
                    FirstName = row.FirstName,
                    LastName = row.LastName
                };

                if (!EntityValidation.IsValid(account))
                {
                    _logger.LogWarning("Account row contained invalid data");
                    continue;
                }

                accounts.Add(account);
            }

            var successfulRows = accounts.Count;
            var failedRows = totalRows - successfulRows;
            if (accounts.Count == 0)
            {
                _logger.LogWarning("CSV contained no valid rows");
                return new ImportResult(0, totalRows);
            }

            try
            {
                await _accountsRepository.InsertAccounts([.. accounts], cancellationToken);
            }
            catch (Exception dbEx)
            {
                _logger.LogError(dbEx, "Database error during batch insert of accounts");
                return new ImportResult(0, totalRows);
            }
            return new ImportResult(successfulRows, failedRows);
        }
    }
}
