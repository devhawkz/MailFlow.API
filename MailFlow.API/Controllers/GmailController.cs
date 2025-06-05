using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Gmail.v1;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

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
            var credential = await GetUserCredentialAsync();

            var userId = Guid.Parse("02d9cd73-990c-437c-827b-fac07e08ba09");

            // sends a request to Google API to get new access token only when access token is nearly expired, in other case it doesn't send a request to Google API
            await credential.RefreshTokenAsync(CancellationToken.None);

            var expiresAt = DateTime.UtcNow.AddSeconds(credential.Token.ExpiresInSeconds ?? 3600);

            var existing = await _context.GoogleTokens
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (existing is not null)
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

        [HttpGet("labels")]
        public async Task<IActionResult> GetLabels()
        {
            var userId = Guid.Parse("02d9cd73-990c-437c-827b-fac07e08ba09"); // user seed

            var token = await _context.GoogleTokens
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.ExpiresAt)
                .FirstOrDefaultAsync();

            if (token is null)
                return Unauthorized("Access token not found.");
       
            if (token.ExpiresAt < DateTime.UtcNow)
            {
                var credential = await GetUserCredentialAsync();
                await credential.RefreshTokenAsync(CancellationToken.None);

                token.AccessToken = credential.Token.AccessToken;
                token.RefreshToken = credential.Token.RefreshToken;
                token.ExpiresAt = DateTime.UtcNow.AddSeconds(credential.Token.ExpiresInSeconds ?? 3600);

                _context.GoogleTokens.Update(token);

               
            }

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token.AccessToken);

            var response = await httpClient.GetAsync("https://gmail.googleapis.com/gmail/v1/users/me/labels");
            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());

            var content = await response.Content.ReadAsStringAsync();
           

            var labelList = JsonSerializer.Deserialize<GmailLabelListResponse>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            foreach(var label in labelList.Labels)
            {
                var exists = await _context.GmailLabels.AnyAsync(l => l.Id == label.Id && l.UserId == userId);
                if(exists)
                    continue;
                label.UserId = userId; // set the UserId for each label
                await _context.GmailLabels.AddAsync(label);
            }
            await _context.SaveChangesAsync();

            return Ok(labelList.Labels);

           
        }

        [HttpGet("emails/{labelId}")]
        public async Task<IActionResult> GetEmailsByLabels(string labelId)
        {
            if(string.IsNullOrEmpty(labelId))
                return BadRequest("Label ID is required.");

            var userId = Guid.Parse("02d9cd73-990c-437c-827b-fac07e08ba09"); // user seed

            var token = await _context.GoogleTokens
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.ExpiresAt)
                .FirstOrDefaultAsync();

            if(token == null)
                return Unauthorized("Access token not found.");

            if(token.ExpiresAt < DateTime.UtcNow)
            {
                var credential = await GetUserCredentialAsync();
                await credential.RefreshTokenAsync(CancellationToken.None);
                token.AccessToken = credential.Token.AccessToken;
                token.RefreshToken = credential.Token.RefreshToken;
                token.ExpiresAt = DateTime.UtcNow.AddSeconds(credential.Token.ExpiresInSeconds ?? 3600);
                _context.GoogleTokens.Update(token);
                await _context.SaveChangesAsync();
            }

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token.AccessToken);

            // Fetching list of emails ids based on labelId, maximum 5 emails 
            var listResponse = await httpClient.GetAsync($"https://gmail.googleapis.com/gmail/v1/users/me/messages?labelIds={labelId}&maxResults=5");

            if(!listResponse.IsSuccessStatusCode)
                return StatusCode((int)listResponse.StatusCode, await listResponse.Content.ReadAsStringAsync());

            var listJson = await listResponse.Content.ReadAsStringAsync();
            var listData = JsonSerializer.Deserialize<GmailMessageListResponse>(listJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            //first part checks if list is null and second part checks if list contains at least one email
            if (listData.Messages is null || !listData.Messages.Any())
                return Ok("No emails found for this label.");

            // Fetching details of each email based on the email id
            var savedlMessages = new List<EmailMessage>();
            var labelName = await _context.GmailLabels
                .Where(l => l.Id == labelId && l.UserId == userId)
                .Select(l => l.Name)
                .FirstOrDefaultAsync();

            foreach (var messageHeader in listData.Messages)
            {
                var detailResponse = await httpClient.GetAsync(
                    $"https://gmail.googleapis.com/gmail/v1/users/me/messages/{messageHeader.Id}?format=full");

                if (!detailResponse.IsSuccessStatusCode)
                    continue;

                var detailJson = await detailResponse.Content.ReadAsStringAsync();
                var detailData = JsonSerializer.Deserialize<GmailMessageDetail>(detailJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var subject = detailData.Payload.Headers.FirstOrDefault(h => h.Name.Equals("Subject"))?.Value;
                var from = detailData.Payload.Headers.FirstOrDefault(h => h.Name.Equals("From"))?.Value;
                var receivedAt = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(detailData.InternalDate)).DateTime;

                // maybe this check need to be moved to the top of the loop
                var exists = await _context.EmailMessages.AnyAsync(e => e.GmailMessageId == detailData.Id);
                if (exists)
                    continue;

                var emailMessage = new EmailMessage
                {
                    Id = Guid.NewGuid(),
                    GmailMessageId = detailData.Id,
                    Subject = subject,
                    From = from,
                    Snippet = detailData.Snippet,
                    ReceivedAt = receivedAt,
                    LabelName = labelId,
                    UserId = userId // set the UserId for each email
                };

                await _context.EmailMessages.AddAsync(emailMessage);
                savedlMessages.Add(emailMessage);

            }

            await _context.SaveChangesAsync();


            IEnumerable<EmailMessage> savedMessages = await _context.EmailMessages
                .Where(e => e.UserId == userId && e.LabelName == labelId)
                .ToListAsync();



            return Ok(savedMessages);
        }

        [HttpGet("emails/{emailId}/body")]
        public async Task<IActionResult> GetEmailBodyById(string emailId)
        {
            if (string.IsNullOrEmpty(emailId))
                return BadRequest("EmailId is required.");

            var userId = Guid.Parse("02d9cd73-990c-437c-827b-fac07e08ba09");

            var token = await _context.GoogleTokens
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.ExpiresAt)
                .FirstOrDefaultAsync();

            if (token == null)
                return Unauthorized("Access token not found.");

            if (token.ExpiresAt < DateTime.UtcNow)
            {
                var credential = await GetUserCredentialAsync();
                await credential.RefreshTokenAsync(CancellationToken.None);
                token.AccessToken = credential.Token.AccessToken;
                token.RefreshToken = credential.Token.RefreshToken;
                token.ExpiresAt = DateTime.UtcNow.AddSeconds(credential.Token.ExpiresInSeconds ?? 3600);
                _context.GoogleTokens.Update(token);
                
            }

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token.AccessToken);

            var url = $"https://gmail.googleapis.com/gmail/v1/users/me/messages/{emailId}?format=full";
            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());

            var json = await response.Content.ReadAsStringAsync();
            var message = JsonSerializer.Deserialize<GmailMessageDetail>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            string content = null;
            string format = null;

            if (message.Payload?.Body?.Data is not null)
            {
                content = DecodeBase64UrlString(message.Payload.Body.Data);
                format = "text/plain";
            }
            else if (message.Payload?.Parts is not null)
            {
                var textPart = message.Payload.Parts
                    .FirstOrDefault(p => p.MimeType == "text/plain" && p.Body?.Data is not null);

                if (textPart is not null)
                {
                    content = DecodeBase64UrlString(textPart.Body.Data);
                    format = "text/plain";
                }
                else
                {
                    var htmlPart = message.Payload.Parts
                        .FirstOrDefault(p => p.MimeType == "text/html" && p.Body?.Data is not null);

                    if (htmlPart is not null)
                    {
                        content = DecodeBase64UrlString(htmlPart.Body.Data);
                        format = "text/html";
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(content))
                return Ok(new { format = "none", content = "Telo mejla nije pronađeno ili nije podržano." });

            // Preuzmi EmailMessage entitet
            var emailEntity = await _context.EmailMessages
                .FirstOrDefaultAsync(e => e.GmailMessageId == emailId && e.UserId == userId);

            if (emailEntity is null)
                return NotFound("EmailMessage za ovaj emailId nije pronađen u bazi.");

            // Proveri da li već postoji sadržaj za ovaj mejl
            var existingContent = await _context.EmailMessageContents
                .FirstOrDefaultAsync(c => c.EmailMessageId == emailEntity.Id);

            if (existingContent is null)
            {
                var contentEntity = new EmailMessageContent
                {
                    Id = Guid.NewGuid(),
                    EmailMessageId = emailEntity.Id,
                    Content = content
                };

                await _context.EmailMessageContents.AddAsync(contentEntity);
               
            }
            await _context.SaveChangesAsync();
            return Ok(new
            {
                format,
                content
            });
        }

        // HELPER METHODS
        private async Task<UserCredential> GetUserCredentialAsync()
        {
            var clientId = _config["GmailClientId"];
            var clientSecret = _config["GmailClientSecret"];

            return await GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                },
                new[] { GmailService.Scope.GmailReadonly },
                "user", //location where the token will be stored is associated with this identifier
                CancellationToken.None,
                new NullDataStore() // name of the folder where the token will be stored and token file location
            );
        }

        private static string DecodeBase64UrlString(string base64Url)
        {
            base64Url = base64Url.Replace('-', '+').Replace('_', '/');
            switch (base64Url.Length % 4)
            {
                case 2: base64Url += "=="; break;
                case 3: base64Url += "="; break;

                //low risk of exception because we use GmailAPI, but still need to handle it
                case 1:
                    throw new FormatException("Invalid base64url string: Length modulo 4 cannot be 1.");
            }
            var bytes = Convert.FromBase64String(base64Url);
            return Encoding.UTF8.GetString(bytes);
        }

    }

    //ENTITIES
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
        public string LabelName { get; set; }
        public EmailMessageContent Content { get; set; } // 1:1 relation to EmailMessageContent

        public Guid UserId { get; set; } // foreign key to User table
        public User User { get; set; } // navigation property to User table

       
    }

    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; }

        public ICollection<GoogleToken> GoogleTokens { get; set; }
        public ICollection<EmailMessage> EmailMessages { get; set; }
        public ICollection<GmailLabel> GmailLabels { get; set; }
    }

    public class GmailLabel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

        public Guid UserId { get; set; } // foreign key to User table
        public User User { get; set; } // navigation property to User table
    }
    public class EmailMessageContent
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        [ForeignKey("EmailMessage")]
        public Guid EmailMessageId { get; set; } // foreign key to EmailMessage table
        public EmailMessage EmailMessage { get; set; } // navigation property to EmailMessage table
    }

    //DTOs
    public record GmailLabelListResponse(IEnumerable<GmailLabel> Labels);
   
    public record GmailMessageListResponse(IEnumerable<GmailMessageHeader> Messages);

    public class GmailMessageHeader
    {
        public string Id { get; set; }
        public string ThreadId { get; set; }
    }
    public class GmailMessageDetail
    {
        public string Id { get; set; }
        public string ThreadId { get; set; }
        public IEnumerable<string> LabelIds { get; set; }
        public string Snippet { get; set; }
        public string InternalDate { get; set; }
        public GmailPayload Payload { get; set; }
    }

    public class GmailPayload
    {
        public GmailBody Body { get; set; }
        public List<GmailHeader> Headers { get; set; }
        public List<GmailPart> Parts { get; set; }
    }

    public class GmailPart
    {
        public string MimeType { get; set; }
        public GmailBody Body { get; set; }
    }

    public class GmailBody
    {
        public string Data { get; set; }
    }

    public class GmailHeader
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    // DBCONTEXT CLASS
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
           
        }

        public DbSet<GoogleToken> GoogleTokens { get; set; }
        public DbSet<EmailMessage> EmailMessages { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<GmailLabel> GmailLabels {  get; set; }
        public DbSet<EmailMessageContent> EmailMessageContents { get; set; }


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

            // 1:1 relation EmailMessage and EmailMessageContent
            modelBuilder.Entity<EmailMessage>()
                .HasOne(e => e.Content)
                .WithOne(c => c.EmailMessage)
                .HasForeignKey<EmailMessageContent>(c => c.EmailMessageId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }

}
