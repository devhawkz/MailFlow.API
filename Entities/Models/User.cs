namespace Entities.Models;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; }

    public ICollection<GoogleToken> GoogleTokens { get; set; }
    public ICollection<EmailMessage> EmailMessages { get; set; }
    public ICollection<GmailLabel> GmailLabels { get; set; }
}
