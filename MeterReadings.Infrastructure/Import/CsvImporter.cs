using CsvHelper;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Globalization;

namespace MeterReadings.Infrastructure.Import
{
    public class CsvImporter
    {
        private readonly ImmutableList<ICsvHandler> _csvHandlers;
        private readonly ILogger<CsvImporter> _logger;

        public CsvImporter(IEnumerable<ICsvHandler> csvHandlers, ILogger<CsvImporter> logger)
        {
            _csvHandlers = [.. csvHandlers];
            _logger = logger;
        }

        public async Task<ImportResult> ImportFromStreamAsync(Stream csvStream, CancellationToken cancellationToken = default)
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
                throw new Exception("Failed to read CSV header");
            }

            if (csvReader.HeaderRecord is null)
            {
                _logger.LogError("CSV file is empty or missing headers");
                throw new NotSupportedException("CSV file is empty or is header record is null");
            }

            var headers = csvReader.HeaderRecord.ToImmutableArray();
            foreach (var csvHandler in _csvHandlers)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (csvHandler.CanParse(headers))
                {
                    var importResults = await csvHandler.ImportAsync(csvReader, cancellationToken);
                    return importResults;
                }
            }

            throw new NotSupportedException("Header format not supported");
        }
    }
}
