using MeterReadings.Infrastructure.Import;
using MeterReadings.Infrastructure.Import.MeterReadings;

namespace MeterReadings.API.Services
{
    public class UploadMeterReadingsHandler
    {
        private readonly CsvImporter _csvImporter;
        private readonly ILogger<UploadMeterReadingsHandler> _logger;
        private readonly MeterReadingsCsvHandler _csvHandler;

        public UploadMeterReadingsHandler(CsvImporter csvImporter, MeterReadingsCsvHandler csvHandler, ILogger<UploadMeterReadingsHandler> logger)
        {
            _csvImporter = csvImporter;
            _csvHandler = csvHandler;
            _logger = logger;
        }

        public async Task<ImportResult> HandleAsync(Stream meterReadingsStream, CancellationToken cancellation)
        {
            return await _csvImporter.ImportFromStreamAsync(meterReadingsStream, _csvHandler, cancellation);
        }
    }
}
