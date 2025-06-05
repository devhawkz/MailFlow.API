using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models;

public class GmailLabel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }

    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; } // foreign key to User table
    public User User { get; set; } // navigation property to User table
}
