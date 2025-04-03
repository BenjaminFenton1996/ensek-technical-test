namespace MeterReadings.Infrastructure.Initialization
{
    public class DatabaseBootstrapper
    {
        private readonly EnergyCompanyDbContext _context;
        public DatabaseBootstrapper(EnergyCompanyDbContext context)
        {
            _context = context;
        }

        public async Task<bool> RunBootstrapper()
        {
            await _context.Database.EnsureCreatedAsync();
            if (!_context.Accounts.Any())
            {
                //TODO - Seed with data from Test_Accounts
            }
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
