using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Service.Contracts;
using Shared.DTOs;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MailFlow.API.Controllers
{
    [Route("api/gmail")]
    [ApiController]
    public class GmailController : ControllerBase
    {

        private readonly IServiceManager _serviceManager;
        public GmailController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
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
        public async Task<ActionResult<GmailLabelListDTO>> GetLabels()
        { 
           var response = await _serviceManager.GmailLabelService.GetLabelsFromAPI(trackChanges: false);
            if (response is null || !response.Labels.Any())
                return NotFound("No labels found.");
            return Ok(response);
        }

        [HttpGet("emails/{labelId}")]
        public async Task<IActionResult> GetEmailsByLabels(string labelId)
        {
            if (string.IsNullOrEmpty(labelId))
                return BadRequest("Label ID is required.");

            var userId = Guid.Parse("02d9cd73-990c-437c-827b-fac07e08ba09"); // user seed

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
                await _context.SaveChangesAsync();
            }

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token.AccessToken);

            // Fetching list of emails ids based on labelId, maximum 5 emails 
            var listResponse = await httpClient.GetAsync($"https://gmail.googleapis.com/gmail/v1/users/me/messages?labelIds={labelId}&maxResults=5");

            if (!listResponse.IsSuccessStatusCode)
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
      

    }

    //ENTITIES


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



}
