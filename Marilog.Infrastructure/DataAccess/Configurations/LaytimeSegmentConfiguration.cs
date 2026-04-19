using Marilog.Domain.Entities.LaytimeEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class LaytimeSegmentConfiguration : IEntityTypeConfiguration<LaytimeSegment>
    {
        public void Configure(EntityTypeBuilder<LaytimeSegment> builder)
        {
            builder.ToTable("LaytimeSegments");

            // ─── Primary Key ───────────────────────────────────────────────
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            // ─── Scalar Properties ─────────────────────────────────────────
            builder.Property(x => x.LaytimeCalculationId).IsRequired();

            builder.Property(x => x.From).IsRequired();
            builder.Property(x => x.To).IsRequired();

            builder.Property(x => x.ImpactType)
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .IsRequired();

            builder.Property(x => x.Factor)
                   .HasPrecision(5, 2)
                   .IsRequired();

            builder.Property(x => x.Reason)
                   .HasMaxLength(300)
                   .IsRequired(false);

            // ─── Computed — لا تُخزَّن ─────────────────────────────────────
            builder.Ignore(x => x.Duration);
            builder.Ignore(x => x.CountedDuration);

            // ─── Indexes ───────────────────────────────────────────────────
            builder.HasIndex(x => x.LaytimeCalculationId)
                   .HasDatabaseName("IX_LaytimeSegments_CalculationId");
        }
    }
}
