using CsvHelper;
using System.Collections.Immutable;

namespace MeterReadings.Infrastructure.Import
{
    internal static class CsvImportBatchParser<TRow>
    {
        //TODO - Tuple is getting a bit chunky here, makes consuming it a bit annoying, probably better now to put create a class/record and return that instead
        public static async Task<(ImmutableList<TRow> parsedRows, int totalRows, bool isLastBatch)> ReadCsvAsync(CsvReader csv, int batchSize, Func<CsvReader, TRow?> rowParser)
        {
            var totalRows = 0;
            var parsedRows = new List<TRow>();

            while (await csv.ReadAsync())
            {
                totalRows++;
                var parsedRow = rowParser.Invoke(csv);
                if (parsedRow is null)
                {
                    continue;
                }

                parsedRows.Add(parsedRow);

                if (batchSize == totalRows)
                {
                    return ([.. parsedRows], totalRows, false);
                }
            }
            return ([.. parsedRows], totalRows, true);
        }
    }
}
