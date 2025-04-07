using MeterReadings.Infrastructure;
using MeterReadings.Infrastructure.Exceptions;
using MeterReadings.Infrastructure.Import;
using MeterReadings.Infrastructure.Import.Accounts;
using MeterReadings.Infrastructure.Import.MeterReadings;
using MeterReadings.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Globalization;
using System.Text;

namespace MeterReadings.Tests.IntegrationTests
{
    [TestFixture]
    internal class CsvImporterTests
    {
        private CsvBatchImporter _csvImporter;
        private AccountsCsvHandler _accountsCsvHandler;
        private MeterReadingsCsvHandler _meterReadingsCsvHandler;
        private EnergyCompanyDbContext _context;

        [OneTimeSetUp]
        public async Task OneTimeSetup()
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
            var meterReadingsRepository = new MeterReadingsRepository(_context);

            var accountsCsvHandlerLoggerMock = new Mock<ILogger<AccountsCsvHandler>>();
            _accountsCsvHandler = new AccountsCsvHandler(accountsRepository, accountsCsvHandlerLoggerMock.Object);

            var meterReadingsCsvHandlerLoggerMock = new Mock<ILogger<MeterReadingsCsvHandler>>();
            _meterReadingsCsvHandler = new MeterReadingsCsvHandler(meterReadingsRepository, _context, meterReadingsCsvHandlerLoggerMock.Object);

            var csvImporterLoggerMock = new Mock<ILogger<CsvBatchImporter>>();
            _csvImporter = new CsvBatchImporter(csvImporterLoggerMock.Object);
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
        public async Task TestBigImport()
        {
            var accountsCsvBuilder = new StringBuilder();
            accountsCsvBuilder.Append("AccountId,FirstName,LastName");
            for (int i = 1; i <= 54623; i++)
            {
                accountsCsvBuilder.Append($"\r\n{i},Tommy,Test");
            }

            var accountsCsv = accountsCsvBuilder.ToString();
            using var accountsStream = new MemoryStream(Encoding.UTF8.GetBytes(accountsCsv));

            await foreach (var importBatch in _csvImporter.ImportFromStreamAsync(accountsStream, _accountsCsvHandler, 100))
            {
                await _context.SaveChangesAsync();
            }

            var accounts = _context.Accounts.ToArray();
            Assert.That(accounts, Has.Length.EqualTo(54623));
            await accountsStream.DisposeAsync();
        }

        [Test]
        public async Task TestValidImport()
        {
            var accountsCsv = "AccountId,FirstName,LastName\r\n2344,Tommy,Test\r\n2233,Barry,Test";
            using var accountsStream = new MemoryStream(Encoding.UTF8.GetBytes(accountsCsv));

            var actualImportAccountResults = (await _csvImporter.ImportFromStreamAsync(accountsStream, _accountsCsvHandler, 100).ToListAsync()).FirstOrDefault();
            var expectedImportAccountResults = new ImportBatchResult(2, 0, true);
            Assert.That(actualImportAccountResults, Is.EqualTo(expectedImportAccountResults));

            await _context.SaveChangesAsync();
            var accounts = _context.Accounts.ToArray();
            var expectedAccount = accounts.First(x => x.AccountId == 2233);
            Assert.Multiple(() =>
            {
                Assert.That(accounts, Has.Length.EqualTo(2));
                Assert.That(expectedAccount.AccountId, Is.EqualTo(2233));
                Assert.That(expectedAccount.FirstName, Is.EqualTo("Barry"));
                Assert.That(expectedAccount.LastName, Is.EqualTo("Test"));
            });
            await accountsStream.DisposeAsync();

            var meterReadingsCsv = "AccountId,MeterReadingDateTime,MeterReadValue,\r\n2344,22/04/2019 09:24,1002,\r\n2233,22/04/2019 12:25,323,";
            using var meterReadingsStream = new MemoryStream(Encoding.UTF8.GetBytes(meterReadingsCsv));
            var actualImportMeterReadingsResults = (await _csvImporter.ImportFromStreamAsync(meterReadingsStream, _meterReadingsCsvHandler, 100).ToListAsync()).FirstOrDefault();
            var expectedImportMeterReadingsResults = new ImportBatchResult(2, 0, true);
            Assert.That(actualImportMeterReadingsResults, Is.EqualTo(expectedImportMeterReadingsResults));

            await _context.SaveChangesAsync();
            var meterReadings = _context.MeterReadings.ToArray();
            var barryMeterReading = meterReadings.First(x => x.AccountId == 2233);
            Assert.Multiple(() =>
            {
                Assert.That(meterReadings, Has.Length.EqualTo(2));
                Assert.That(barryMeterReading.AccountId, Is.EqualTo(2233));
                Assert.That(barryMeterReading.MeterReadingDateTime, Is.EqualTo(DateTimeOffset.ParseExact("22/04/2019 12:25", "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None).UtcDateTime));
                Assert.That(barryMeterReading.MeterReadValue, Is.EqualTo("00323"));
            });
        }

        [Test]
        public async Task TestPartiallyValidImport()
        {
            var accountsCsv = "AccountId,FirstName,LastName\r\n2344,Tommy,Test\r\n2233,,Test";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(accountsCsv));

            var actualImportResults = (await _csvImporter.ImportFromStreamAsync(stream, _accountsCsvHandler, 100).ToListAsync()).FirstOrDefault();
            var expectedImportResults = new ImportBatchResult(1, 1, true);
            Assert.That(actualImportResults, Is.EqualTo(expectedImportResults));

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
            Assert.ThrowsAsync<CsvHeaderException>(async () =>
            {
                await _csvImporter.ImportFromStreamAsync(stream, _accountsCsvHandler, 100).ToListAsync();
            });

            await _context.SaveChangesAsync();
            var accounts = _context.Accounts.ToArray();
            Assert.That(accounts, Has.Length.EqualTo(0));
        }
    }
}
