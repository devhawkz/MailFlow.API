using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models;

public class EmailMessageContent
{
    public Guid Id { get; set; }
    public string Content { get; set; }
    [ForeignKey(nameof(EmailMessage))]
    public Guid EmailMessageId { get; set; } // foreign key to EmailMessage table
    public EmailMessage EmailMessage { get; set; } // navigation property to EmailMessage table
}
