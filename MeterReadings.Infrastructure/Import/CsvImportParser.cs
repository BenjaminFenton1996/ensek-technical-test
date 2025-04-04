using CsvHelper;
using System.Collections.Immutable;

namespace MeterReadings.Infrastructure.Import
{
    internal static class CsvImportParser<TRow>
    {
        public static async Task<(ImmutableList<TRow> parsedRows, int totalRows)> ReadCsvAsync(CsvReader csv, Func<CsvReader, TRow?> rowParser)
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
            }
            return ([.. parsedRows], totalRows);
        }
    }
}
