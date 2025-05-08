using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MailFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GmailController : ControllerBase
    {
        private readonly IConfiguration _config;
        public GmailController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("check-secrets")]
        public IActionResult CheckSecrets()
        {
           var clientId = _config["Gmail:ClientId"];
           var clientSecret = _config["Gmail:ClientSecret"];
           return Ok(new { clientId, clientSecret });
        }
    }
}
