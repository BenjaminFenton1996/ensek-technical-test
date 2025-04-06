using MeterReadings.API.DTOs;
using MeterReadings.Infrastructure;
using MeterReadings.Infrastructure.Import;
using MeterReadings.Infrastructure.Import.MeterReadings;
using Microsoft.EntityFrameworkCore;

namespace MeterReadings.API.Services
{
    public class ImportMeterReadingsService : IImportMeterReadingsService
    {
        private readonly CsvImporter _csvImporter;
        private readonly ILogger<ImportMeterReadingsService> _logger;
        private readonly EnergyCompanyDbContext _context;
        private readonly MeterReadingsCsvHandler _csvHandler;

        public ImportMeterReadingsService(CsvImporter csvImporter, MeterReadingsCsvHandler csvHandler, EnergyCompanyDbContext context, ILogger<ImportMeterReadingsService> logger)
        {
            _csvImporter = csvImporter;
            _csvHandler = csvHandler;
            _context = context;
            _logger = logger;
        }

        public async Task<ImportMeterReadingsResponse> HandleImportAsync(IFormFile csvFile, CancellationToken cancellation)
        {
            using var meterReadingsStream = csvFile.OpenReadStream();
            var importResults = await _csvImporter.ImportFromStreamAsync(meterReadingsStream, _csvHandler, cancellation);
            if (importResults.SuccessfulRows > 0)
            {
                try
                {
                    await _context.SaveChangesAsync(cancellation);
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LogError(dbEx, "Failed to save meter readings to the db");
                    throw;
                }
            }
            return new ImportMeterReadingsResponse(importResults.SuccessfulRows, importResults.FailedRows);
        }
    }
}
