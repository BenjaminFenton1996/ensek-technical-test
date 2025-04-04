namespace MeterReadings.API.DTOs
{
    public record UploadMeterReadingsResponse(int SuccessfulRows, int FailedRows);
}
