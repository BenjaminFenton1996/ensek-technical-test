namespace MeterReadings.Infrastructure.Import.MeterReadings
{
    internal record MeterReadingImportRow(int AccountId, DateTime MeterReadingDateTime, string MeterReadValue);
}
