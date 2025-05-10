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
        private readonly DataContext _context;
        public GmailController(IConfiguration config, DataContext context)
        {
            _config = config;
            _context = context;
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
                "user", 
                CancellationToken.None
            );

            var userId = Guid.Parse("02d9cd73-990c-437c-827b-fac07e08ba09");

            // sends a request to Google API to get new access token only when access token is nearly expired, in other case it doesn't send a request to Google API
            await credential.RefreshTokenAsync(CancellationToken.None);           

            var expiresAt = DateTime.UtcNow.AddSeconds(credential.Token.ExpiresInSeconds ?? 3600);

            var existing = await _context.GoogleTokens
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (existing != null)
            {
                existing.AccessToken = credential.Token.AccessToken;
                existing.RefreshToken = credential.Token.RefreshToken;
                existing.ExpiresAt = expiresAt;
            }
            else
            {
                var newToken = new GoogleToken
                {
                    Id = Guid.NewGuid(),
                    AccessToken = credential.Token.AccessToken,
                    RefreshToken = credential.Token.RefreshToken,
                    ExpiresAt = expiresAt,
                    UserId = userId // assuming you have a user ID to associate with the token
                };
                await _context.GoogleTokens.AddAsync(newToken);

                
            }

            await _context.SaveChangesAsync();

            return Ok();
        }
    }

    public class GoogleToken
    {
        public Guid Id { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }

        public Guid UserId { get; set; } // foreign key to User table
        public User User { get; set; } // navigation property to User table
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
        
        public Guid UserId { get; set; } // foreign key to User table
        public User User { get; set; } // navigation property to User table
    }

    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; }

        public ICollection<GoogleToken> GoogleTokens { get; set; }
        public ICollection<EmailMessage> EmailMessages { get; set; }
    }



    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
           
        }

        public DbSet<GoogleToken> GoogleTokens { get; set; }
        public DbSet<EmailMessage> EmailMessages { get; set; }
        public DbSet<User> Users { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);

                entity.Property(u => u.Email).IsRequired().HasMaxLength(255);

                entity.HasData(new User
                {
                    Id = Guid.Parse("02d9cd73-990c-437c-827b-fac07e08ba09"),
                    Email = "pavlejovanovic34@gmail.com"
                });

            });
        }
    }

}
