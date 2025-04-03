using Microsoft.AspNetCore.Mvc;

namespace MeterReadings.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MeterReadingController : ControllerBase
    {
        [HttpPost("meter-reading-uploads")]
        public ActionResult MeterReadingUploads()
        {
            //TODO - Implement this
            throw new NotImplementedException();
        }
    }
}
