using System.Collections.Immutable;

namespace MeterReadings.Infrastructure.Import
{
    public record ImportResult(int SuccessfulRows, int FailedRows);
}
