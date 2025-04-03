using CsvHelper;

namespace MeterReadings.Infrastructure.Import.Accounts
{
    internal static class AccountsCsvRowParser
    {
        public static (AccountCsvRow? accountCsvRow, string? errorMessage) ParseRow(IReaderRow csvRow)
        {
            var isValidAccountId = int.TryParse(csvRow.GetField<string>(AccountCsvValidColumns.AccountIdHeader), out var accountId);
            var firstName = csvRow.GetField<string>(AccountCsvValidColumns.FirstNameHeader);
            var lastName = csvRow.GetField<string>(AccountCsvValidColumns.LastNameHeader);

            if (!isValidAccountId)
            {
                return (null, "Account ID is invalid or missing");
            }
            else if (string.IsNullOrEmpty(firstName))
            {
                return (null, "First name cannot be empty");
            }
            else if (string.IsNullOrEmpty(lastName))
            {
                return (null, "Last name cannot be empty");
            }

            return (new AccountCsvRow(accountId, firstName, lastName), null);
        }
    }
}
