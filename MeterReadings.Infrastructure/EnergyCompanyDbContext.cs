using MeterReadings.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeterReadings.Infrastructure
{
    public class EnergyCompanyDbContext : DbContext
    {
        public EnergyCompanyDbContext(DbContextOptions<EnergyCompanyDbContext> options) : base(options) { }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<MeterReading> MeterReadings { get; set; }
    }
}
