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
        [TestCase("AccountId,MeterReadingDateTime,MeterReadValue\r\n2344,01/04/2019 09:24,1002", 2344, "01/04/2019 09:24", "01002")]
        [TestCase("AccountId,MeterReadingDateTime,MeterReadValue\r\n9,22/09/2019 03:05,0", 9, "22/09/2019 03:05", "00000")]
        [TestCase("AccountId,MeterReadingDateTime,MeterReadValue\r\n734441,11/12/2026 14:30,92", 734441, "11/12/2026 14:30", "00092")]
        public async Task TestParsingValidRow(string accountsCsv, int expectedAccountId, string expectedMeterReadDateTimeString, string expectedMeterReadValue)
        {
            using var accountsStream = new MemoryStream(Encoding.UTF8.GetBytes(accountsCsv));
            using var streamReader = new StreamReader(accountsStream);
            using var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture);
            await csvReader.ReadAsync();
            csvReader.ReadHeader();
            await csvReader.ReadAsync();

            var actualRow = MeterReadingsCsvRowParser.ParseRow(csvReader);
            _ = DateTimeOffset.TryParseExact(expectedMeterReadDateTimeString, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var expectedMeterReadingDateTime);
            var expectedRow = new MeterReadingImportRow(expectedAccountId, expectedMeterReadingDateTime.UtcDateTime, expectedMeterReadValue);
            Assert.Multiple(() =>
            {
                Assert.That(actualRow, Is.Not.Null);
                Assert.That(actualRow, Is.EqualTo(expectedRow));
            });
        }

        [TestCase("AccountId,MeterReadingDateTime,MeterReadValue\r\nID,22/044/2019 09:24,1002")]
        [TestCase("AccountId,MeterReadingDateTime,MeterReadValue\r\n941,1st January,1002")]
        [TestCase("AccountId,MeterReadingDateTime,MeterReadValue\r\n,04/22/2019 14:24,1002")]
        [TestCase("AccountId,MeterReadingDateTime,MeterReadValue\r\n1240,01/04/2019 09:24,")]
        public async Task TestParsingInvalidRow(string accountsCsv)
        {
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
