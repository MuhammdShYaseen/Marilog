using Marilog.Ship.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Ship.Data.Configurations
{
    public class CrewExtraConfiguration : IEntityTypeConfiguration<CrewExtra>
    {
        public void Configure(EntityTypeBuilder<CrewExtra> builder)
        {
            builder.ToTable("CrewExtras");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Type)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasMaxLength(20);
            builder.Property(x => x.Reason).IsRequired().HasMaxLength(500);

            builder.HasOne(x => x.CrewContract)
                   .WithMany()
                   .HasForeignKey(x => x.CrewContractId)
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