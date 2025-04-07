using CsvHelper;
using System.Collections.Immutable;

namespace MeterReadings.Infrastructure.Import
{
    public interface ICsvHandler
    {
        public bool CanParse(ImmutableArray<string> headers);
        public Task<ImportBatchResult> ImportAsync(CsvReader csvReader, int batchSize, CancellationToken cancellationToken);
    }
}
