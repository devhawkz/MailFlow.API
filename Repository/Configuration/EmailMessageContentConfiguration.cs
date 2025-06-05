using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace Repository.Configuration;

public class EmailMessageContentConfiguration : IEntityTypeConfiguration<EmailMessageContent>
{
    public void Configure(EntityTypeBuilder<EmailMessageContent> builder)
    {
        builder
            .HasOne(c => c.EmailMessage)
            .WithOne(e => e.Content)
            .HasForeignKey<EmailMessageContent>(c => c.EmailMessageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
