using CsvHelper;
using MeterReadings.Infrastructure.Import.Accounts;
using System.Globalization;
using System.Text;

namespace MeterReadings.Tests.UnitTests
{
    [TestFixture]
    internal class AccountsCSvRowParserTests
    {
        [Test]
        public async Task TestParsingValidRow()
        {
            var accountsCsv = "AccountId,FirstName,LastName\r\n2344,Tommy,Test";
            using var accountsStream = new MemoryStream(Encoding.UTF8.GetBytes(accountsCsv));
            using var streamReader = new StreamReader(accountsStream);
            using var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture);
            await csvReader.ReadAsync();
            csvReader.ReadHeader();
            await csvReader.ReadAsync();

            var actualRow = AccountsCsvRowParser.ParseRow(csvReader);
            var expectedRow = new AccountImportRow(2344, "Tommy", "Test");
            Assert.Multiple(() =>
            {
                Assert.That(actualRow, Is.Not.Null);
                Assert.That(actualRow, Is.EqualTo(expectedRow));
            });
        }

        [Test]
        public async Task TestParsingInvalidRow()
        {
            var accountsCsv = "AccountId,FirstName,LastName\r\nBob,44,Test";
            using var accountsStream = new MemoryStream(Encoding.UTF8.GetBytes(accountsCsv));
            using var streamReader = new StreamReader(accountsStream);
            using var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture);
            await csvReader.ReadAsync();
            csvReader.ReadHeader();
            await csvReader.ReadAsync();

            var actualRow = AccountsCsvRowParser.ParseRow(csvReader);
            Assert.That(actualRow, Is.Null);
        }
    }
}
