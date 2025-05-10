using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

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
            var clientId = _config["GmailClientId"];
            var clientSecret = _config["GmailClientSecret"];

            // find a better way to check this conditions
            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                return BadRequest("Gmail ClientId or ClientSecret is missing.");

            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                },
                new[] { GmailService.Scope.GmailReadonly },
                "user", //location where the token will be stored is associated with this identifier
                CancellationToken.None,
                new FileDataStore("Gmail.Auth.Store", true) // name of the folder where the token will be stored and token file location
            );
            
            // sends a request to Google API to get new access token only when access token is nearly expired, in other case it doesn't send a request to Google API
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
           var clientId = _config["GmailClientId"];
           var clientSecret = _config["GmailClientSecret"];
           return Ok(new { clientId, clientSecret });
        }
    }

    public class GoogleToken
    {
        public Guid Id { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; } // navigation property
    }

    public class  EmailMessage
    {
        public Guid Id { get; set; }
        public string GmailMessageId { get; set; }
        public string Subject { get; set; }
        public string From { get; set; }
        public string Snippet { get; set; }
        public DateTime ReceivedAt { get; set; }

        // JSON list of Gmail label ids, enables us to save original Gmail label ids for each email (in json format) in order to be able to filter, search, group emails by labels
        public string LabelIdJson { get; set; }
        
        public Guid UserId { get; set; }
        public User User { get; set; } // navigation property
    }

    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; }

        public ICollection<GoogleToken> GoogleTokens { get; set; } = new List<GoogleToken>();
        public ICollection<EmailMessage> EmailMessages { get; set; } = new List<EmailMessage>();
    }



    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
           
        }

        public DbSet<GoogleToken> GoogleTokens { get; set; }
        public DbSet<EmailMessage> EmailMessages { get; set; }
        public DbSet<User> Users { get; set; }
    }

}
