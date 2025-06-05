
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repository.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasData
        (
             new User
             {
                 Id = Guid.Parse("02d9cd73-990c-437c-827b-fac07e08ba09"),
                 Email = "pavlejovanovic34@gmail.com"
             }
        );
    }
}

