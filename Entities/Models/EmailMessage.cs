using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models;
public class EmailMessage
{
    public Guid Id { get; set; }
    public string GmailMessageId { get; set; }
    public string Subject { get; set; }
    public string From { get; set; }
    public string Snippet { get; set; }
    public DateTime ReceivedAt { get; set; }
    public string LabelName { get; set; }
    public EmailMessageContent Content { get; set; } // 1:1 relation to EmailMessageContent

    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; } // foreign key to User table
    public User User { get; set; } // navigation property to User table
}
