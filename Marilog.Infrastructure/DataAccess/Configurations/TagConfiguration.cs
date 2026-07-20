using Marilog.Domain.Entities.SystemEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class TagConfiguration : IEntityTypeConfiguration<Tag>
    {
        public void Configure(EntityTypeBuilder<Tag> builder)
        {
            builder.ToTable("Tags");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(t => t.Color)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("#1E88E5");



            builder.Property(t => t.StoredFileId)
                .IsRequired();

            builder.HasIndex(t => new { t.StoredFileId, t.Name })
                .IsUnique();
        }
    }
}
