using Marilog.Ship.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Ship.Data.Configurations
{
    public class OwnerRemittanceConfiguration : IEntityTypeConfiguration<OwnerRemittance>
    {
        public void Configure(EntityTypeBuilder<OwnerRemittance> builder)
        {
            builder.ToTable("OwnerRemittances");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Type)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasMaxLength(30);
            builder.Property(x => x.Reference).HasMaxLength(100);
            builder.Property(x => x.Notes).HasMaxLength(500);

            builder.HasOne(x => x.Currency)
                   .WithMany()
                   .HasForeignKey(x => x.CurrencyId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CrewContract)
                   .WithMany()
                   .HasForeignKey(x => x.CrewContractId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Port)
                   .WithMany()
                   .HasForeignKey(x => x.PortId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.Date);
            builder.HasIndex(x => x.CrewContractId);
        }
    }
}