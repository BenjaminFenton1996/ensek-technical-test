using MeterReadings.API.DTOs;
using MeterReadings.Infrastructure;
using MeterReadings.Infrastructure.Import;
using MeterReadings.Infrastructure.Import.MeterReadings;
using Microsoft.EntityFrameworkCore;

namespace MeterReadings.API.Services
{
    public class ImportMeterReadingsService : IImportMeterReadingsService
    {
        private readonly CsvBatchImporter _csvImporter;
        private readonly ILogger<ImportMeterReadingsService> _logger;
        private readonly EnergyCompanyDbContext _context;
        private readonly MeterReadingsCsvHandler _csvHandler;

        public ImportMeterReadingsService(CsvBatchImporter csvImporter, MeterReadingsCsvHandler csvHandler, EnergyCompanyDbContext context, ILogger<ImportMeterReadingsService> logger)
        {
            _csvImporter = csvImporter;
            _csvHandler = csvHandler;
            _context = context;
            _logger = logger;
        }

        public async Task<ImportMeterReadingsResponse> HandleImportAsync(IFormFile csvFile, CancellationToken cancellationToken)
        {
            using var meterReadingsStream = csvFile.OpenReadStream();

            var totalSuccessfulRows = 0;
            var totalFailedRows = 0;
            await foreach (var importBatch in _csvImporter.ImportFromStreamAsync(meterReadingsStream, _csvHandler, 100, cancellationToken))
            {
                totalSuccessfulRows += importBatch.SuccessfulRows;
                totalFailedRows += importBatch.FailedRows;
                if (importBatch.SuccessfulRows > 0)
                {
                    try
                    {
                        await _context.SaveChangesAsync(cancellationToken);
                    }
                    catch (DbUpdateException dbEx)
                    {
                        _logger.LogError(dbEx, "Failed to save meter readings to the db");
                        throw;
                    }
                }
            }

            return new ImportMeterReadingsResponse(totalSuccessfulRows, totalFailedRows);
        }
    }
}
