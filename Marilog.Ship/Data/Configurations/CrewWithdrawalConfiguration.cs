using Marilog.Ship.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Ship.Data.Configurations
{
    public class CrewWithdrawalConfiguration : IEntityTypeConfiguration<CrewWithdrawal>
    {
        public void Configure(EntityTypeBuilder<CrewWithdrawal> builder)
        {
            builder.ToTable("CrewWithdrawals");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Type)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasMaxLength(20);
            builder.Property(x => x.Reason).HasMaxLength(500);

            builder.HasOne(x => x.CrewContract)
                   .WithMany()
                   .HasForeignKey(x => x.CrewContractId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.DeductionType)
                   .WithMany()
                   .HasForeignKey(x => x.DeductionTypeId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Currency)
                   .WithMany()
                   .HasForeignKey(x => x.CurrencyId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.Date);
            builder.HasIndex(x => new { x.CrewContractId, x.Date });
        }
    }
}