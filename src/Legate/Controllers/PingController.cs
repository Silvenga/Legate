using Microsoft.AspNetCore.Mvc;

namespace Legate.Controllers
{
    [Route("api/v1/ping")]
    public class PingController : Controller
    {
        [HttpGet]
        public IActionResult Ping()
        {
            return Ok();
        }
    }
}