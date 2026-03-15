using Marilog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class CrewPayrollConfiguration : IEntityTypeConfiguration<CrewPayroll>
    {
        public void Configure(EntityTypeBuilder<CrewPayroll> builder)
        {
            builder.ToTable("CrewPayrolls");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).UseIdentityColumn();
            builder.Property(x => x.ContractId).IsRequired();
            builder.Property(x => x.PayrollMonth).IsRequired().HasColumnType("date");
            builder.Property(x => x.WorkingDays).IsRequired();

            // ── Amounts (USD) ─────────────────────────────────────────────────────
            builder.Property(x => x.BasicWage)
                   .IsRequired()
                   .HasColumnType("decimal(12,2)")
                   .HasComment("USD");
            builder.Property(x => x.Allowances)
                   .IsRequired()
                   .HasColumnType("decimal(12,2)")
                   .HasDefaultValue(0m)
                   .HasComment("USD");
            builder.Property(x => x.Deductions)
                   .IsRequired()
                   .HasColumnType("decimal(12,2)")
                   .HasDefaultValue(0m)
                   .HasComment("USD");
            builder.Property(x => x.GrossAmount)
                   .IsRequired()
                   .HasColumnType("decimal(12,2)")
                   .HasComment("USD — BasicWage + Allowances - Deductions");

            builder.Property(x => x.Status)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .HasDefaultValue(PayrollStatus.Draft);

            builder.Property(x => x.Notes).HasMaxLength(500);
            builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

            // ── Relations ─────────────────────────────────────────────────────────
            builder.HasOne(x => x.Contract)
                   .WithMany()
                   .HasForeignKey(x => x.ContractId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Disbursements)
                   .WithOne()
                   .HasForeignKey(x => x.PayrollId)
                   .OnDelete(DeleteBehavior.Cascade);

            // ── One payroll per contract per month (DB-level guarantee) ───────────
            builder.HasIndex(x => new { x.ContractId, x.PayrollMonth })
                   .IsUnique();

            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.PayrollMonth);
        }
    }
}
