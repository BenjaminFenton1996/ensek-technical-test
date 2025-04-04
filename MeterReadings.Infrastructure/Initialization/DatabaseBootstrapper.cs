using MeterReadings.Infrastructure.Import;
using MeterReadings.Infrastructure.Import.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MeterReadings.Infrastructure.Initialization
{
    public class DatabaseBootstrapper
    {
        private readonly EnergyCompanyDbContext _context;
        private readonly CsvImporter _csvImporter;
        private readonly AccountsCsvHandler _accountsCsvHandler;
        private readonly ILogger<DatabaseBootstrapper> _logger;

        public DatabaseBootstrapper(EnergyCompanyDbContext context, CsvImporter csvImporter, AccountsCsvHandler accountsCsvHandler, ILogger<DatabaseBootstrapper> logger)
        {
            _context = context;
            _csvImporter = csvImporter;
            _accountsCsvHandler = accountsCsvHandler;
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
                        await _csvImporter.ImportFromStreamAsync(csvFileStream, _accountsCsvHandler, cancellationToken);
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

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "An error occurred when trying to save changes after bootstrapping the database");
                throw;
            }
        }
    }
}
