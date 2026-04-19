using Marilog.Domain.Entities.LaytimeEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class LaytimeCalculationConfiguration
         : IEntityTypeConfiguration<LaytimeCalculation>
    {
        public void Configure(EntityTypeBuilder<LaytimeCalculation> builder)
        {
            builder.ToTable("LaytimeCalculations");

            // ─── Primary Key ───────────────────────────────────────────────
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            // ─── Scalar Properties ─────────────────────────────────────────
            builder.Property(x => x.VoyageId).IsRequired();
            builder.Property(x => x.ContractId).IsRequired();
            builder.Property(x => x.PortId).IsRequired();

            builder.Property(x => x.OperationType)
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .IsRequired();

            builder.Property(x => x.Status)
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .IsRequired();

            builder.Property(x => x.CargoQuantityMt)
                   .HasPrecision(18, 3)
                   .IsRequired();

            builder.Property(x => x.LaytimeCommencedAt).IsRequired(false);
            builder.Property(x => x.LaytimeCompletedAt).IsRequired(false);

            // ─── LaytimeResult (Owned Value Object — nullable) ─────────────
            builder.OwnsOne(x => x.Result, r =>
            {
                r.Property(p => p.AllowedDays)
                 .HasColumnName("Result_AllowedDays")
                 .HasPrecision(18, 4);

                r.Property(p => p.UsedDays)
                 .HasColumnName("Result_UsedDays")
                 .HasPrecision(18, 4);

                r.Property(p => p.BalanceDays)
                 .HasColumnName("Result_BalanceDays")
                 .HasPrecision(18, 4);

                r.Property(p => p.DemurrageAmount)
                 .HasColumnName("Result_DemurrageAmount")
                 .HasPrecision(18, 2);

                r.Property(p => p.DespatchAmount)
                 .HasColumnName("Result_DespatchAmount")
                 .HasPrecision(18, 2);

                r.Property(p => p.IsDemurrage)
                 .HasColumnName("Result_IsDemurrage");
            });

            // ─── Navigations ───────────────────────────────────────────────
            builder.HasMany(x => x.SofEvents)
                   .WithOne()
                   .HasForeignKey(x => x.LaytimeCalculationId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Segments)
                   .WithOne()
                   .HasForeignKey(x => x.LaytimeCalculationId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Exceptions)
                   .WithOne()
                   .HasForeignKey(x => x.LaytimeCalculationId)
                   .OnDelete(DeleteBehavior.Cascade);

            // ─── Indexes ───────────────────────────────────────────────────
            builder.HasIndex(x => x.VoyageId)
                   .HasDatabaseName("IX_LaytimeCalculations_VoyageId");

            builder.HasIndex(x => x.ContractId)
                   .HasDatabaseName("IX_LaytimeCalculations_ContractId");

            builder.HasIndex(x => new { x.VoyageId, x.OperationType })
                   .HasDatabaseName("IX_LaytimeCalculations_Voyage_Operation");
        }
    }
}
