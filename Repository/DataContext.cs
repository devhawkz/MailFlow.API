using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repository.Configuration;

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

        modelBuilder.ApplyConfiguration(new UserConfiguration());
    }
}
