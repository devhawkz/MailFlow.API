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

        [HttpGet("authorize")]
        public async Task<IActionResult> Authorize()
        {
            var clientId = _config["Gmail:ClientId"];
            var clientSecret = _config["Gmail:ClientSecret"];

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                return BadRequest("Gmail ClientId or ClientSecret is missing.");

            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                },
                new[] { GmailService.Scope.GmailReadonly },
                "user",
                CancellationToken.None,
                new FileDataStore("Gmail.Auth.Store", true)
            );

            await credential.RefreshTokenAsync(CancellationToken.None);

            return Ok(new
            {
                AccessToken = credential.Token.AccessToken,
                RefreshToken = credential.Token.RefreshToken,
                TokenType = credential.Token.TokenType,
                ExpiresIn = credential.Token.ExpiresInSeconds,
                Issued = credential.Token.IssuedUtc
            });
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
