using Microsoft.AspNetCore.Mvc;

namespace MeterReadingsAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MeterReadingController : ControllerBase
    {
        [HttpPost("meter-reading-uploads")]
        public ActionResult MeterReadingUploads()
        {
            //TODO - Upload meter readings
            throw new NotImplementedException();
        }
    }
}
