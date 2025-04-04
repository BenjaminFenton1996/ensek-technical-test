using MeterReadings.Infrastructure.Entities;
using System.Collections.Immutable;

namespace MeterReadings.Infrastructure.Repositories
{
    internal class MeterReadingsRepository : IMeterReadingsRepository
    {
        private readonly EnergyCompanyDbContext _context;
        public MeterReadingsRepository(EnergyCompanyDbContext context)
        {
            _context = context;
        }

        public async Task InsertMeterReadings(ImmutableList<MeterReading> meterReadings, CancellationToken cancellationToken = default)
        {
            await _context.MeterReadings.AddRangeAsync(meterReadings, cancellationToken);
        }
    }
}
