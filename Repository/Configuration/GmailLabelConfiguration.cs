using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Configuration
{
    internal class GmailLabelConfiguration : IEntityTypeConfiguration<GmailLabel>
    {
        public void Configure(EntityTypeBuilder<GmailLabel> builder)
        {
            builder.HasIndex(l => l.UserId)
              .HasName("GmailLabels_UserId");
        }
          

    }
}
