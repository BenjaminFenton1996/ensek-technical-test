using CsvHelper;
using MeterReadings.Infrastructure.Entities;
using MeterReadings.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;

namespace MeterReadings.Infrastructure.Import.Accounts
{
    internal class AccountsCsvHandler : ICsvHandler
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
            var successfulRows = 0;
            var failedRows = 0;
            var errors = new List<string>();
            var accounts = new List<Account>();

            while (await csv.ReadAsync())
            {
                cancellationToken.ThrowIfCancellationRequested();
                var (accountCsvRow, errorMessage) = AccountsCsvRowParser.ParseRow(csv);
                if (errorMessage is not null)
                {
                    failedRows++;
                    errors.Add(errorMessage);
                    continue;
                }
                if (accountCsvRow is null)
                {
                    failedRows++;
                    errors.Add("Account CSV row was null");
                    continue;
                }

                var account = new Account
                {
                    AccountId = accountCsvRow.AccountId,
                    FirstName = accountCsvRow.FirstName,
                    LastName = accountCsvRow.LastName
                };
                accounts.Add(account);
                successfulRows++;
            }

            var totalRows = successfulRows + failedRows;
            if (accounts.Count == 0)
            {
                _logger.LogWarning("CSV contained no valid rows");
                return new ImportResult(totalRows, 0, failedRows, [.. errors]);
            }

            try
            {
                await _accountsRepository.InsertAccounts([.. accounts], cancellationToken);
            }
            catch (Exception dbEx)
            {
                _logger.LogError(dbEx, "Database error during batch insert of accounts");
                errors.Add($"Database error when inserting accounts: {dbEx.Message}");
                return new ImportResult(totalRows, 0, totalRows, [.. errors]);
            }
            return new ImportResult(totalRows, successfulRows, failedRows, [.. errors]);
        }
    }
}
