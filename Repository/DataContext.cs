using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Repository;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {

    }

    public DbSet<GoogleToken>? GoogleTokens { get; set; }
    public DbSet<EmailMessage>? EmailMessages { get; set; }
    public DbSet<User>? Users { get; set; }
    public DbSet<GmailLabel>? GmailLabels { get; set; }
    public DbSet<EmailMessageContent>? EmailMessageContents { get; set; }

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
