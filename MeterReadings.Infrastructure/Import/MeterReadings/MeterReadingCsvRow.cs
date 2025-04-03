namespace MeterReadings.Infrastructure.Import.MeterReadings
{
    internal record MeterReadingCsvRow(int AccountId, DateTime MeterReadingDateTime, int MeterReadValue);
}
