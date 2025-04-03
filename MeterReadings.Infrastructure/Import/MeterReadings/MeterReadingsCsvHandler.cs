using CsvHelper;
using System.Collections.Immutable;

namespace MeterReadings.Infrastructure.Import.MeterReadings
{
    internal class MeterReadingsCsvHandler : ICsvHandler
    {
        public bool CanParse(ImmutableArray<string> headers)
        {
            if (headers is [
                MeterReadingsCsvValidColumns.AccountIdHeader,
                MeterReadingsCsvValidColumns.MeterReadingDateTimeHeader,
                MeterReadingsCsvValidColumns.MeterReadValueHeader
                ])
            {
                return true;
            }
            return false;
        }

        public async Task<ImportResult> ImportAsync(CsvReader csvReader, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
