using CsvHelper;

namespace MeterReadings.Infrastructure.Import.Accounts
{
    internal static class AccountsCsvRowParser
    {
        public static AccountImportRow? ParseRow(IReaderRow csvRow)
        {
            var isValidAccountId = int.TryParse(csvRow.GetField<string>(AccountCsvValidColumns.AccountIdHeader), out var accountId);
            var firstName = csvRow.GetField<string>(AccountCsvValidColumns.FirstNameHeader);
            var lastName = csvRow.GetField<string>(AccountCsvValidColumns.LastNameHeader);

            if (!isValidAccountId)
            {
                return null;
            }
            else if (string.IsNullOrEmpty(firstName))
            {
                return null;
            }
            else if (string.IsNullOrEmpty(lastName))
            {
                return null;
            }

            return new AccountImportRow(accountId, firstName, lastName);
        }
    }
}
