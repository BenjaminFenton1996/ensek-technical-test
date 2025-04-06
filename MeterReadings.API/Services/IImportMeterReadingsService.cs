using MeterReadings.API.DTOs;

namespace MeterReadings.API.Services
{
    public interface IImportMeterReadingsService
    {
        public Task<ImportMeterReadingsResponse> HandleImportAsync(IFormFile csvFile, CancellationToken cancellation);
    }
}
