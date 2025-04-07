namespace MeterReadings.Infrastructure.Import
{
    public record ImportBatchResult(int SuccessfulRows, int FailedRows, bool IsLastBatch);
}
