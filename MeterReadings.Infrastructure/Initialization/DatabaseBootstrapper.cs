using MeterReadings.Infrastructure.Import;
using Microsoft.Extensions.Logging;

namespace MeterReadings.Infrastructure.Initialization
{
    public class DatabaseBootstrapper
    {
        private readonly EnergyCompanyDbContext _context;
        private readonly CsvImporter _csvImporter;
        private readonly ILogger<DatabaseBootstrapper> _logger;

        public DatabaseBootstrapper(EnergyCompanyDbContext context, CsvImporter csvImporter, ILogger<DatabaseBootstrapper> logger)
        {
            _context = context;
            _csvImporter = csvImporter;
            _logger = logger;
        }

        public async Task RunBootstrapper(bool isDevelopmentEnv, CancellationToken cancellationToken = default)
        {
            await _context.Database.EnsureCreatedAsync(cancellationToken);
            if (isDevelopmentEnv && !_context.Accounts.Any())
            {
                _logger.LogInformation("Starting database account seeding");
                var testAccountsRelativePath = Path.Combine("DevResources", "Test_Accounts.csv");
                var testAccountsPath = Path.Combine(AppContext.BaseDirectory, testAccountsRelativePath);

                if (File.Exists(testAccountsPath))
                {
                    try
                    {
                        using var csvFileStream = new FileStream(testAccountsPath, FileMode.Open, FileAccess.Read);
                        await _csvImporter.ImportFromStreamAsync(csvFileStream, cancellationToken);
                        _logger.LogInformation("Finished account seeding");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "An error occurred when trying to import test accounts to the database");
                    }
                }
                else
                {
                    _logger.LogWarning($"No test accounts file was found with the provided path: {testAccountsPath}");
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
