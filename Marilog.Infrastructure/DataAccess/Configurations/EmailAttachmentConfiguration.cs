using Marilog.Domain.Entities.SystemEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class EmailAttachmentConfiguration : IEntityTypeConfiguration<EmailAttachment>
    {
        public void Configure(EntityTypeBuilder<EmailAttachment> builder)
        {
            builder.ToTable("EmailAttachments");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).UseIdentityColumn();
            builder.Property(x => x.FileName).IsRequired().HasMaxLength(255);
            builder.Property(x => x.FilePath).IsRequired().HasMaxLength(500);
            builder.Property(x => x.FileSizeBytes).IsRequired();

            builder.HasIndex(x => x.EmailId);
        }
    }
}
