using MeterReadings.Infrastructure.Import;

namespace MeterReadings.API.Services
{
    public interface IUploadMeterReadingsService
    {
        public Task<ImportResult> HandleAsync(IFormFile csvFile, CancellationToken cancellation);
    }
}
