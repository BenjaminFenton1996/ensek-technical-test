using CsvHelper;
using MeterReadings.Infrastructure.Import.Accounts;
using System.Globalization;
using System.Text;

namespace MeterReadings.Tests.UnitTests
{
    [TestFixture]
    internal class AccountsCSvRowParserTests
    {
        [TestCase("AccountId,FirstName,LastName\r\n2344,Tommy,Test", 2344, "Tommy", "Test")]
        [TestCase("AccountId,FirstName,LastName\r\n70112,Bob,Turk", 70112, "Bob", "Turk")]
        [TestCase("AccountId,FirstName,LastName\r\n9,Jeremy,Karl", 9, "Jeremy", "Karl")]
        public async Task TestParsingValidRow(string accountsCsv, int expectedId, string expectedFirstName, string expectedLastName)
        {
            using var accountsStream = new MemoryStream(Encoding.UTF8.GetBytes(accountsCsv));
            using var streamReader = new StreamReader(accountsStream);
            using var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture);
            await csvReader.ReadAsync();
            csvReader.ReadHeader();
            await csvReader.ReadAsync();

            var actualRow = AccountsCsvRowParser.ParseRow(csvReader);
            var expectedRow = new AccountImportRow(expectedId, expectedFirstName, expectedLastName);
            Assert.Multiple(() =>
            {
                Assert.That(actualRow, Is.Not.Null);
                Assert.That(actualRow, Is.EqualTo(expectedRow));
            });
        }

        [TestCase("AccountId,FirstName,LastName\r\nBob,44,Test")]
        [TestCase("AccountId,FirstName,LastName\r\n,44,Test")]
        [TestCase("AccountId,FirstName,LastName\r\n,,")]
        public async Task TestParsingInvalidRow(string accountsCsv)
        {
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
