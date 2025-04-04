using CsvHelper;
using System.Globalization;

namespace MeterReadings.Infrastructure.Import.MeterReadings
{
    internal static class MeterReadingsCsvRowParser
    {
        public static MeterReadingImportRow? ParseRow(IReaderRow csvRow)
        {
            var accountIdField = csvRow.GetField<string>(MeterReadingsCsvValidColumns.AccountIdHeader);
            var isValidAccountId = int.TryParse(accountIdField, out var accountId);

            var meterReadingDateTimeField = csvRow.GetField<string>(MeterReadingsCsvValidColumns.MeterReadingDateTimeHeader);
            var isValidMeterReadingDateTime = DateTimeOffset.TryParseExact(meterReadingDateTimeField, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var meterReadingDateTime);

            var meterReadValueHeader = csvRow.GetField<string>(MeterReadingsCsvValidColumns.MeterReadValueHeader);
            var isValidmeterReadValue = int.TryParse(meterReadValueHeader, out var meterReadValue);

            if (!isValidAccountId)
            {
                return null;
            }
            else if (!isValidMeterReadingDateTime)
            {
                return null;
            }
            else if (!isValidmeterReadValue)
            {
                return null;
            }

            return new MeterReadingImportRow(accountId, meterReadingDateTime.UtcDateTime, meterReadValue.ToString("D5"));
        }
    }
}
