using CsvHelper;
using MeterReadings.Infrastructure.Import;
using MeterReadings.Infrastructure.Import.Accounts;
using System.Globalization;
using System.Text;

namespace MeterReadings.Tests.UnitTests
{
    [TestFixture]
    internal class CsvImportParserTests
    {
        [Test]
        public async Task TestParsingCsv()
        {
            var accountsCsv = "AccountId,FirstName,LastName\r\n2344,Tommy,Test\r\n2233,Barry,Test";
            using var accountsStream = new MemoryStream(Encoding.UTF8.GetBytes(accountsCsv));
            using var streamReader = new StreamReader(accountsStream);
            using var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture);
            await csvReader.ReadAsync();
            csvReader.ReadHeader();

            var (parsedRows, totalRows, isLastBatch) = await CsvImportBatchParser<AccountImportRow>.ReadCsvAsync(csvReader, 100, AccountsCsvRowParser.ParseRow);
            var expectedRow = new AccountImportRow(2344, "Tommy", "Test");

            Assert.Multiple(() =>
            {
                Assert.That(parsedRows, Is.Not.Null);
                Assert.That(parsedRows, Has.Count.EqualTo(2));
                Assert.That(totalRows, Is.EqualTo(2));
                Assert.That(parsedRows[0], Is.EqualTo(expectedRow));
            });
        }

        [Test]
        public async Task TestParsingPartiallyValidCsv()
        {
            var accountsCsv = "AccountId,FirstName,LastName\r\n,Tommy,Test\r\n2233,Barry,Test";
            using var accountsStream = new MemoryStream(Encoding.UTF8.GetBytes(accountsCsv));
            using var streamReader = new StreamReader(accountsStream);
            using var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture);
            await csvReader.ReadAsync();
            csvReader.ReadHeader();

            var (parsedRows, totalRows, isLastBatch) = await CsvImportBatchParser<AccountImportRow>.ReadCsvAsync(csvReader, 100, AccountsCsvRowParser.ParseRow);
            var expectedRow = new AccountImportRow(2233, "Barry", "Test");

            Assert.Multiple(() =>
            {
                Assert.That(parsedRows, Is.Not.Null);
                Assert.That(parsedRows, Has.Count.EqualTo(1));
                Assert.That(totalRows, Is.EqualTo(2));
                Assert.That(parsedRows[0], Is.EqualTo(expectedRow));
            });
        }

        [Test]
        public async Task TestParsingInvalidCsv()
        {
            var accountsCsv = "AccountId,FirstName,LastName\r\n,,\r\n,Barry,";
            using var accountsStream = new MemoryStream(Encoding.UTF8.GetBytes(accountsCsv));
            using var streamReader = new StreamReader(accountsStream);
            using var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture);
            await csvReader.ReadAsync();
            csvReader.ReadHeader();

            var (parsedRows, totalRows, isLastBatch) = await CsvImportBatchParser<AccountImportRow>.ReadCsvAsync(csvReader, 100, AccountsCsvRowParser.ParseRow);
            var expectedRow = new AccountImportRow(2344, "Tommy", "Test");

            Assert.Multiple(() =>
            {
                Assert.That(parsedRows, Is.Empty);
                Assert.That(totalRows, Is.EqualTo(2));
            });
        }
    }
}
