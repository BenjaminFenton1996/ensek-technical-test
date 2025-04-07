using CsvHelper;
using MeterReadings.Infrastructure.Exceptions;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace MeterReadings.Infrastructure.Import
{
    public class CsvBatchImporter
    {
        private readonly ILogger<CsvBatchImporter> _logger;

        public CsvBatchImporter(ILogger<CsvBatchImporter> logger)
        {
            _logger = logger;
        }

        public async IAsyncEnumerable<ImportBatchResult> ImportFromStreamAsync(Stream csvStream, ICsvHandler csvHandler, int batchSize, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting import");
            ArgumentNullException.ThrowIfNull(csvStream);
            if (!csvStream.CanRead)
            {
                throw new ArgumentException("Stream must be readable", nameof(csvStream));
            }

            using var streamReader = new StreamReader(csvStream, leaveOpen: true);
            using var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture);

            try
            {
                await csvReader.ReadAsync();
                csvReader.ReadHeader();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading CSV header");
                throw new CsvHeaderException("Failed to read CSV header");
            }

            if (csvReader.HeaderRecord is null)
            {
                _logger.LogError("CSV file is empty or missing headers");
                throw new CsvHeaderException("CSV file is empty or header record is null");
            }

            var headers = csvReader.HeaderRecord.ToImmutableArray();
            if (!csvHandler.CanParse(headers))
            {
                _logger.LogError("CSV header format is not supported");
                throw new CsvHeaderException("Header format not supported");
            }

            _logger.LogInformation("Starting import batch loop");
            while (true)
            {
                var importBatch = await csvHandler.ImportAsync(csvReader, batchSize, cancellationToken);
                if (importBatch == null || importBatch.SuccessfulRows == 0 && importBatch.FailedRows == 0)
                {
                    _logger.LogInformation("ImportAsync returned no processed rows, assuming end of data");
                    break;
                }

                yield return importBatch;
                if (importBatch.IsLastBatch)
                {
                    _logger.LogInformation("IsLastBatch flag is true, ending import batch loop");
                    break;
                }
            }
        }
    }
}
