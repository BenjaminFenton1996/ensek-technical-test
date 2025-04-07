using CsvHelper;
using MeterReadings.Infrastructure.Entities;
using MeterReadings.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Data.Common;

namespace MeterReadings.Infrastructure.Import.MeterReadings
{
    public class MeterReadingsCsvHandler : ICsvHandler
    {
        private readonly IMeterReadingsRepository _meterReadingsRepository;
        private readonly EnergyCompanyDbContext _context;
        private readonly ILogger<MeterReadingsCsvHandler> _logger;
        public MeterReadingsCsvHandler(IMeterReadingsRepository neterReadingsRepository, EnergyCompanyDbContext context, ILogger<MeterReadingsCsvHandler> logger)
        {
            _meterReadingsRepository = neterReadingsRepository;
            _context = context;
            _logger = logger;
        }

        public bool CanParse(ImmutableArray<string> headers)
        {
            if (headers is [MeterReadingsCsvValidColumns.AccountIdHeader, MeterReadingsCsvValidColumns.MeterReadingDateTimeHeader, MeterReadingsCsvValidColumns.MeterReadValueHeader, ..])
            {
                return true;
            }
            return false;
        }

        public async Task<ImportBatchResult> ImportAsync(CsvReader csv, int batchSize, CancellationToken cancellationToken = default)
        {
            var existingAccountIds = new HashSet<int>();
            try
            {
                existingAccountIds = [.. _context.Accounts.Select(x => x.AccountId)];
            }
            catch (DbException ex)
            {
                _logger.LogError(ex, "Failed to get existing account IDs");
            }

            var (parsedRows, totalRows, isLastBatch) = await CsvImportBatchParser<MeterReadingImportRow>.ReadCsvAsync(csv, batchSize, MeterReadingsCsvRowParser.ParseRow);
            var meterReadings = new List<MeterReading>();
            try
            {
                var existingReadings = _context.MeterReadings.Where(x => parsedRows.Select(pr => pr.AccountId).Contains(x.AccountId));
                var existingReadingsLookup = existingReadings.ToLookup(x => x.AccountId);
                foreach (var row in parsedRows)
                {
                    if (!existingAccountIds.Contains(row.AccountId))
                    {
                        _logger.LogWarning("Account ID for meter reading does not exist: {accountId}", row.AccountId);
                        continue;
                    }

                    var meterReading = new MeterReading
                    {
                        AccountId = row.AccountId,
                        MeterReadingDateTime = row.MeterReadingDateTime,
                        MeterReadValue = row.MeterReadValue
                    };

                    if (!EntityValidation.IsValid(meterReading))
                    {
                        _logger.LogWarning("Meter reading row contained invalid data");
                        continue;
                    }

                    var existingAccountReadings = existingReadingsLookup[meterReading.AccountId];
                    var identicalReadingExists = existingAccountReadings.Any(x => x.MeterReadValue == meterReading.MeterReadValue && x.MeterReadingDateTime == meterReading.MeterReadingDateTime);
                    if (existingAccountReadings != null && identicalReadingExists)
                    {
                        _logger.LogWarning("Meter reading is identical to an existing reading");
                        continue;
                    }

                    var meterReadingIsOutdated = existingAccountReadings?.Any(x => x.MeterReadingDateTime >= meterReading.MeterReadingDateTime) ?? false;
                    if (existingAccountReadings != null && meterReadingIsOutdated)
                    {
                        _logger.LogWarning("Meter reading is older than an existing reading");
                        continue;
                    }

                    meterReadings.Add(meterReading);
                }
            }
            catch (DbException ex)
            {
                _logger.LogError(ex, "Failed to get existing meter readings");
            }

            var successfulRows = meterReadings.Count;
            var failedRows = totalRows - successfulRows;
            if (meterReadings.Count == 0)
            {
                _logger.LogWarning("CSV contained no valid rows");
                return new ImportBatchResult(0, totalRows, isLastBatch);
            }

            try
            {
                await _meterReadingsRepository.InsertMeterReadings([.. meterReadings], cancellationToken);
            }
            catch (Exception dbEx)
            {
                _logger.LogError(dbEx, "Database error during batch insert of meter readings");
                return new ImportBatchResult(0, totalRows, isLastBatch);
            }
            return new ImportBatchResult(successfulRows, failedRows, isLastBatch);
        }
    }
}
