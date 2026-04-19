using Marilog.Domain.Entities.LaytimeEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class SofEventConfiguration : IEntityTypeConfiguration<SofEvent>
    {
        public void Configure(EntityTypeBuilder<SofEvent> builder)
        {
            builder.ToTable("SofEvents");

            // ─── Primary Key ───────────────────────────────────────────────
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            // ─── Scalar Properties ─────────────────────────────────────────
            builder.Property(x => x.LaytimeCalculationId).IsRequired();

            builder.Property(x => x.EventTime).IsRequired();

            builder.Property(x => x.EventType)
                   .HasConversion<string>()
                   .HasMaxLength(40)
                   .IsRequired();

            builder.Property(x => x.ImpactType)
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .IsRequired();

            builder.Property(x => x.Factor)
                   .HasPrecision(5, 2)
                   .IsRequired();

            builder.Property(x => x.Description)
                   .HasMaxLength(500)
                   .IsRequired(false);

            // ─── Indexes ───────────────────────────────────────────────────
            // composite index — الـ Engine يرتب الأحداث زمنياً دائماً
            builder.HasIndex(x => new { x.LaytimeCalculationId, x.EventTime })
                   .HasDatabaseName("IX_SofEvents_Calculation_Time");
        }
    }
}
