using Microsoft.AspNetCore.Mvc;

namespace TcpServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ServerInfoController : ControllerBase
    {
        private readonly ILogger<ServerInfoController> _logger;

        public ServerInfoController(ILogger<ServerInfoController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { Message = "TCP Server is running as part of this Web API" });
        }
    }
}
