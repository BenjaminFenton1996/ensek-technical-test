using CsvHelper;

namespace MeterReadings.Infrastructure.Import.MeterReadings
{
    internal static class MeterReadingsCsvRowParser
    {
        public static (MeterReadingCsvRow? meterReadingCsvRow, string? errorMessage) ParseRow(IReaderRow csvRow)
        {
            var isValidAccountId = int.TryParse(csvRow.GetField<string>(MeterReadingsCsvValidColumns.AccountIdHeader), out var accountId);
            if (!isValidAccountId)
            {
                return (null, "Account ID is invalid or missing");
            }

            throw new NotImplementedException();
        }
    }
}
