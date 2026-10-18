using Marilog.Domain.Entities.SystemEntities;
using Marilog.Kernel.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class CrewPayrollDisbursementConfiguration
       : IEntityTypeConfiguration<CrewPayrollDisbursement>
    {
        public void Configure(EntityTypeBuilder<CrewPayrollDisbursement> builder)
        {
            builder.ToTable("CrewPayrollDisbursements");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).UseIdentityColumn();
            builder.Property(x => x.PayrollId).IsRequired();

            builder.Property(x => x.Method)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .HasComment("CashOnBoard | CashAtOffice | BankTransfer");

            builder.Property(x => x.Amount)
                   .IsRequired()
                   .HasColumnType("decimal(12,2)")
                   .HasComment("In the payroll's currency");

            builder.Property(x => x.PaidOn).IsRequired().HasColumnType("date");

            builder.Property(x => x.Status)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .HasDefaultValue(DisbursementStatus.Pending);

            builder.Property(x => x.RecipientName).HasMaxLength(200);
            builder.Property(x => x.RecipientIdNumber).HasMaxLength(50);
            builder.Property(x => x.Reference).HasMaxLength(100);
            builder.Property(x => x.Notes).HasMaxLength(300);
            builder.Property(x => x.CancelReason).HasMaxLength(300);

            builder.HasIndex(x => x.PayrollId);
            builder.HasIndex(x => x.Status);
        }
    }
}
