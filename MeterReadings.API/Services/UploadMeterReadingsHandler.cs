using MeterReadings.Infrastructure;
using MeterReadings.Infrastructure.Import;
using MeterReadings.Infrastructure.Import.MeterReadings;
using Microsoft.EntityFrameworkCore;

namespace MeterReadings.API.Services
{
    public class UploadMeterReadingsHandler
    {
        private readonly CsvImporter _csvImporter;
        private readonly ILogger<UploadMeterReadingsHandler> _logger;
        private readonly EnergyCompanyDbContext _context;
        private readonly MeterReadingsCsvHandler _csvHandler;

        public UploadMeterReadingsHandler(CsvImporter csvImporter, MeterReadingsCsvHandler csvHandler, EnergyCompanyDbContext context, ILogger<UploadMeterReadingsHandler> logger)
        {
            _csvImporter = csvImporter;
            _csvHandler = csvHandler;
            _context = context;
            _logger = logger;
        }

        public async Task<ImportResult> HandleAsync(IFormFile csvFile, CancellationToken cancellation)
        {
            using var meterReadingsStream = csvFile.OpenReadStream();
            var importResults = await _csvImporter.ImportFromStreamAsync(meterReadingsStream, _csvHandler, cancellation);
            if (importResults.SuccessfulRows > 0)
            {
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LogError(dbEx, "Failed to save meter readings to the db");
                    throw;
                }
            }
            return importResults;
        }
    }
}
