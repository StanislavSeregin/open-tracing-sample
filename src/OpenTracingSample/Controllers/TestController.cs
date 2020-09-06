using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace OpenTracingSample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;

        public TestController(ILogger<TestController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogWarning("This is test warning!");
            _logger.LogError("This is test error!");
            _logger.LogCritical("This is test critical!");
            return Ok("Test");
        }
    }
}
