using MeterReadings.API.DTOs;
using MeterReadings.API.Services;
using MeterReadings.Infrastructure;
using MeterReadings.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MeterReadings.API.Controllers
{
    [ApiController]
    public class MeterReadingController : ControllerBase
    {
        private readonly UploadMeterReadingsHandler _uploadMeterReadingsHandler;
        private readonly EnergyCompanyDbContext _context;
        private readonly ILogger<MeterReadingController> _logger;

        public MeterReadingController(UploadMeterReadingsHandler uploadMeterReadingsHandler, EnergyCompanyDbContext context, ILogger<MeterReadingController> logger)
        {
            _uploadMeterReadingsHandler = uploadMeterReadingsHandler;
            _context = context;
            _logger = logger;
        }

        [HttpPost("meter-reading-uploads")]
        [RequestSizeLimit(100 * 1024)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> MeterReadingUploads(IFormFile csvFile, CancellationToken cancellationToken)
        {
            if (csvFile == null || csvFile.Length == 0)
            {
                return BadRequest("No file uploaded or file contents are empty");
            }
            if (csvFile.ContentType != "text/csv")
            {
                return BadRequest("Expected a text/csv file");
            }

            try
            {
                using var stream = csvFile.OpenReadStream();
                var importResults = await _uploadMeterReadingsHandler.HandleAsync(stream, cancellationToken);
                if (importResults.SuccessfulRows > 0)
                {
                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateException dbEx)
                    {
                        _logger.LogError(dbEx, "Failed to save meter readings to the db");
                        return StatusCode(500, "Something went wrong while importing the CSV data");
                    }
                }

                var response = new UploadMeterReadingsResponse(importResults.SuccessfulRows, importResults.FailedRows);
                return Ok(response);
            }
            catch (Exception ex)
            {
                if (ex is CsvHeaderException)
                {
                    return BadRequest("CSV headers were null or invalid");
                }

                _logger.LogError(ex, "An error occurred while importing the CSV data");
                return StatusCode(500, "Something went wrong while importing the CSV data");
            }
        }
    }
}
