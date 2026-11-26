using Marilog.Domain.Entities.SystemEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Ship.Data.Configurations
{
    // Not used on board — child of CrewPayroll (see CrewPayrollConfiguration).
    public class CrewPayrollDisbursementConfiguration : IEntityTypeConfiguration<CrewPayrollDisbursement>
    {
        public void Configure(EntityTypeBuilder<CrewPayrollDisbursement> builder)
        {
            builder.ToTable("CrewPayrollDisbursements");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Method)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasMaxLength(20);
            builder.Property(x => x.Status)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasMaxLength(20);
            builder.Property(x => x.RecipientName).HasMaxLength(200);
            builder.Property(x => x.RecipientIdNumber).HasMaxLength(50);
            builder.Property(x => x.Reference).HasMaxLength(100);
            builder.Property(x => x.Notes).HasMaxLength(300);
            builder.Property(x => x.CancelReason).HasMaxLength(300);

            builder.HasIndex(x => x.PayrollId);
        }
    }
}