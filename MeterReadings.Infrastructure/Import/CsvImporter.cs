using CsvHelper;
using MeterReadings.Infrastructure.Exceptions;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Globalization;

namespace MeterReadings.Infrastructure.Import
{
    public class CsvImporter
    {
        private readonly ILogger<CsvImporter> _logger;

        public CsvImporter(ILogger<CsvImporter> logger)
        {
            _logger = logger;
        }

        public async Task<ImportResult> ImportFromStreamAsync(Stream csvStream, ICsvHandler csvHandler, CancellationToken cancellationToken = default)
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
            if (csvHandler.CanParse(headers))
            {
                var importResults = await csvHandler.ImportAsync(csvReader, cancellationToken);
                return importResults;
            }

            throw new CsvHeaderException("Header format not supported");
        }
    }
}
