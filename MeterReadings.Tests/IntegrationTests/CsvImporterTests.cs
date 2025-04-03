using MeterReadings.Infrastructure;
using MeterReadings.Infrastructure.Import;
using MeterReadings.Infrastructure.Import.Accounts;
using MeterReadings.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;

namespace MeterReadings.Tests.IntegrationTests
{
    [TestFixture]
    internal class CsvImporterTests
    {
        private CsvImporter _csvImporter;
        private EnergyCompanyDbContext _context;

        [OneTimeSetUp]
        public async Task Setup()
        {
            //In a real production system we'd probably want to get the connection string from an env variable so that we can set it and run these integration tests in a CI/CD pipeline, but no need here
            var connectionString = "Server=localhost;Port=5432;User Id=postgres;Password=password;Database=test_energy_company_db;Include Error Detail=true;";
            var builder = new DbContextOptionsBuilder<EnergyCompanyDbContext>();
            builder.UseNpgsql(connectionString);
            var options = builder.Options;
            _context = new EnergyCompanyDbContext(options);
            await _context.Database.EnsureDeletedAsync();
            await _context.Database.EnsureCreatedAsync();

            var accountsRepository = new AccountsRepository(_context);
            var accountsCsvHandlerLoggerMock = new Mock<ILogger<AccountsCsvHandler>>();
            var accountsCsvHandler = new AccountsCsvHandler(accountsRepository, accountsCsvHandlerLoggerMock.Object);
            var csvImporterLoggerMock = new Mock<ILogger<CsvImporter>>();

            _csvImporter = new CsvImporter([accountsCsvHandler], csvImporterLoggerMock.Object);
        }

        [SetUp]
        public async Task Reset()
        {
            _context.Accounts.RemoveRange(_context.Accounts);
            await _context.SaveChangesAsync();
        }

        [OneTimeTearDown]
        public async Task TearDown()
        {
            await _context.Database.EnsureDeletedAsync();
            await _context.DisposeAsync();
        }

        [Test]
        public async Task TestValidAccountsImport()
        {
            var accountsCsv = "AccountId,FirstName,LastName\r\n2344,Tommy,Test\r\n2233,Barry,Test";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(accountsCsv));

            var actualImportResults = await _csvImporter.ImportFromStreamAsync(stream);
            var expectedImportResults = new ImportResult(2, 2, 0, []);
            Assert.Multiple(() =>
            {
                Assert.That(actualImportResults, Is.EqualTo(expectedImportResults));
            });

            await _context.SaveChangesAsync();
            var accounts = _context.Accounts.ToArray();
            var barryAccount = accounts.First(x => x.AccountId == 2233);
            Assert.Multiple(() =>
            {
                Assert.That(accounts, Has.Length.EqualTo(2));
                Assert.That(barryAccount.AccountId, Is.EqualTo(2233));
                Assert.That(barryAccount.FirstName, Is.EqualTo("Barry"));
                Assert.That(barryAccount.LastName, Is.EqualTo("Test"));
            });
        }

        [Test]
        public async Task TestPartiallyValidAccountsImport()
        {
            var accountsCsv = "AccountId,FirstName,LastName\r\n2344,Tommy,Test\r\n2233,,Test";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(accountsCsv));

            var actualImportResults = await _csvImporter.ImportFromStreamAsync(stream);
            var expectedImportResults = new ImportResult(2, 1, 1, ["First name cannot be empty"]);
            Assert.Multiple(() =>
            {
                Assert.That(actualImportResults.SuccessfulRows, Is.EqualTo(expectedImportResults.SuccessfulRows));
                Assert.That(actualImportResults.FailedRows, Is.EqualTo(expectedImportResults.FailedRows));
                Assert.That(actualImportResults.TotalRows, Is.EqualTo(expectedImportResults.TotalRows));
                Assert.That(actualImportResults.Errors, Is.EquivalentTo(expectedImportResults.Errors));
            });

            await _context.SaveChangesAsync();
            var accounts = _context.Accounts.ToArray();
            Assert.Multiple(() =>
            {
                Assert.That(accounts, Has.Length.EqualTo(1));
                Assert.That(accounts[0].AccountId, Is.EqualTo(2344));
                Assert.That(accounts[0].FirstName, Is.EqualTo("Tommy"));
                Assert.That(accounts[0].LastName, Is.EqualTo("Test"));
            });
        }

        [Test]
        public async Task TestInvalidImport()
        {
            var accountsCsv = "AccId,FName,LName\r\n2344,Tommy,Test\r\n2233,Barry,Test";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(accountsCsv));
            Assert.ThrowsAsync<NotSupportedException>(() => _csvImporter.ImportFromStreamAsync(stream));

            await _context.SaveChangesAsync();
            var accounts = _context.Accounts.ToArray();
            Assert.Multiple(() =>
            {
                Assert.That(accounts, Has.Length.EqualTo(0));
            });
        }
    }
}
