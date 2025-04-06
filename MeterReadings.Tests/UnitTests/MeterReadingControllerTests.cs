using MeterReadings.API.Controllers;
using MeterReadings.API.DTOs;
using MeterReadings.API.Services;
using MeterReadings.Infrastructure.Import;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;

namespace MeterReadings.Tests.UnitTests
{
    [TestFixture]
    public class MeterReadingControllerTests
    {
        private MeterReadingController _meterReadingController;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var uploadMeterReadingServiceMock = new Mock<IImportMeterReadingsService>();
            uploadMeterReadingServiceMock
                .Setup(x => x.HandleImportAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ImportMeterReadingsResponse(1, 0));

            var loggerMock = new Mock<ILogger<MeterReadingController>>();
            _meterReadingController = new MeterReadingController(uploadMeterReadingServiceMock.Object, loggerMock.Object);
        }

        [Test]
        public async Task TestValidFormFile()
        {
            var csvFormFile = CreateTestFormFile("someCsvContent", "testFile", "file", "text/csv");
            var response = await _meterReadingController.MeterReadingUploads(csvFormFile, default);
            Assert.That(response, Is.Not.Null);
            Assert.That(response, Is.TypeOf(typeof(OkObjectResult)));
        }

        [Test]
        public async Task TestInvalidFormFile()
        {
            var csvFormFile = CreateTestFormFile("someTextContent", "testFile", "file", "text/plain");
            var response = await _meterReadingController.MeterReadingUploads(csvFormFile, default);
            Assert.That(response, Is.Not.Null);
            Assert.That(response, Is.TypeOf(typeof(BadRequestObjectResult)));

            csvFormFile = CreateTestFormFile("", "testFile", "file", "text/csv");
            response = await _meterReadingController.MeterReadingUploads(csvFormFile, default);
            Assert.That(response, Is.Not.Null);
            Assert.That(response, Is.TypeOf(typeof(BadRequestObjectResult)));
        }

        private static FormFile CreateTestFormFile(string content, string fileName, string formFieldName, string contentType)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);

            var formFile = new FormFile(
                baseStream: stream,
                baseStreamOffset: 0,
                length: stream.Length,
                name: formFieldName,
                fileName: fileName
            )
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };

            return formFile;
        }
    }
}
