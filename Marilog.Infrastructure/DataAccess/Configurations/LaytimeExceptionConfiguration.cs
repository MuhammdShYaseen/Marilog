using Marilog.Domain.Entities.LaytimeEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class LaytimeExceptionConfiguration : IEntityTypeConfiguration<LaytimeException>
    {
        public void Configure(EntityTypeBuilder<LaytimeException> builder)
        {
            builder.ToTable("LaytimeExceptions");

            // ─── Primary Key ───────────────────────────────────────────────
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            // ─── Scalar Properties ─────────────────────────────────────────
            builder.Property(x => x.LaytimeCalculationId).IsRequired();

            builder.Property(x => x.From).IsRequired();
            builder.Property(x => x.To).IsRequired();

            builder.Property(x => x.ExceptionType)
                   .HasConversion<string>()
                   .HasMaxLength(30)
                   .IsRequired();

            builder.Property(x => x.Factor)
                   .HasPrecision(5, 2)
                   .IsRequired();

            builder.Property(x => x.Notes)
                   .HasMaxLength(500)
                   .IsRequired(false);

            builder.Property(x => x.LinkedSofEventId)
                   .IsRequired(false);

            // ─── Computed ──────────────────────────────────────────────────
            builder.Ignore(x => x.Duration);

            // ─── Indexes ───────────────────────────────────────────────────
            builder.HasIndex(x => x.LaytimeCalculationId)
                   .HasDatabaseName("IX_LaytimeExceptions_CalculationId");
        }
    }
}