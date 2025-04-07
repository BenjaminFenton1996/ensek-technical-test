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

        public async Task<ImportBatchResult> ImportAsync(CsvReader csv, int batchSize, CancellationToken cancellationToken = default)
        {
            var (parsedRows, totalRows, isLastBatch) = await CsvImportBatchParser<AccountImportRow>.ReadCsvAsync(csv, batchSize, AccountsCsvRowParser.ParseRow);
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
                return new ImportBatchResult(0, totalRows, isLastBatch);
            }

            try
            {
                await _accountsRepository.InsertAccounts([.. accounts], cancellationToken);
            }
            catch (Exception dbEx)
            {
                _logger.LogError(dbEx, "Database error during batch insert of accounts");
                return new ImportBatchResult(0, totalRows, isLastBatch);
            }
            return new ImportBatchResult(successfulRows, failedRows, isLastBatch);
        }
    }
}
