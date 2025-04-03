using System.Collections.Immutable;

namespace MeterReadings.Infrastructure.Import
{
    public record ImportResult(int TotalRows, int SuccessfulRows, int FailedRows, ImmutableList<string> Errors);
}
