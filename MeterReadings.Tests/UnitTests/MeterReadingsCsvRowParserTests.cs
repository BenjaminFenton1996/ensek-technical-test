using CsvHelper;
using MeterReadings.Infrastructure.Import.Accounts;
using MeterReadings.Infrastructure.Import.MeterReadings;
using System.Globalization;
using System.Text;

namespace MeterReadings.Tests.UnitTests
{
    [TestFixture]
    internal class MeterReadingsCsvRowParserTests
    {
        [Test]
        public async Task TestParsingValidRow()
        {
            var accountsCsv = "AccountId,MeterReadingDateTime,MeterReadValue\r\n2344,22/04/2019 09:24,1002";
            using var accountsStream = new MemoryStream(Encoding.UTF8.GetBytes(accountsCsv));
            using var streamReader = new StreamReader(accountsStream);
            using var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture);
            await csvReader.ReadAsync();
            csvReader.ReadHeader();
            await csvReader.ReadAsync();

            var actualRow = MeterReadingsCsvRowParser.ParseRow(csvReader);
            _ = DateTimeOffset.TryParseExact("22/04/2019 09:24", "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var expectedMeterReadingDateTime);
            var expectedRow = new MeterReadingImportRow(2344, expectedMeterReadingDateTime.UtcDateTime, "01002");
            Assert.Multiple(() =>
            {
                Assert.That(actualRow, Is.Not.Null);
                Assert.That(actualRow, Is.EqualTo(expectedRow));
            });
        }

        [Test]
        public async Task TestParsingInvalidRow()
        {
            var accountsCsv = "AccountId,MeterReadingDateTime,MeterReadValue\r\nID,22/044/2019 09:24,1002";
            using var accountsStream = new MemoryStream(Encoding.UTF8.GetBytes(accountsCsv));
            using var streamReader = new StreamReader(accountsStream);
            using var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture);
            await csvReader.ReadAsync();
            csvReader.ReadHeader();

            var actualRow = MeterReadingsCsvRowParser.ParseRow(csvReader);
            Assert.That(actualRow, Is.Null);
        }
    }
}
