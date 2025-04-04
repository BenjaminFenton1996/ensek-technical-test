using MeterReadings.Infrastructure.Entities;
using System.Collections.Immutable;

namespace MeterReadings.Infrastructure.Repositories
{
    public interface IMeterReadingsRepository
    {
        public Task InsertMeterReadings(ImmutableList<MeterReading> meterReadings, CancellationToken cancellationToken);
    }
}
