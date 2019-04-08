using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Infrastructure.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ReadyController : Controller
    {
        private static bool _isReady;

        public static void ApplicationIsReady()
        {
            _isReady = true;
        }

        [HttpGet("")]
        [HttpHead("")]
        public IActionResult Index()
        {
            var code = _isReady
                ? StatusCodes.Status200OK
                : StatusCodes.Status503ServiceUnavailable;

            return StatusCode(code);
        }
    }
}