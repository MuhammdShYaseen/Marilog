using Marilog.Domain.Entities.SystemEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Ship.Data.Configurations
{
    // Not used on board — mapped only because CrewContractService checks payrolls before
    // deleting or changing a contract. The table stays empty on the ship.
    public class CrewPayrollConfiguration : IEntityTypeConfiguration<CrewPayroll>
    {
        public void Configure(EntityTypeBuilder<CrewPayroll> builder)
        {
            builder.ToTable("CrewPayrolls");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Status)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasMaxLength(20);
            builder.Property(x => x.Notes).HasMaxLength(500);
            builder.Property(x => x.CancelReason).HasMaxLength(300);

            builder.HasOne(x => x.Currency)
                   .WithMany()
                   .HasForeignKey(x => x.CurrencyId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Contract)
                   .WithMany()
                   .HasForeignKey(x => x.ContractId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Disbursements)
                   .WithOne()
                   .HasForeignKey(x => x.PayrollId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.ContractId, x.PayrollMonth })
                   .IsUnique()
                   .HasFilter("\"Status\" <> 'Cancelled'");
        }
    }
}