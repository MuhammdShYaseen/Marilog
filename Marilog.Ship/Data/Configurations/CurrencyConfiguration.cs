using Marilog.Domain.Entities.SystemEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Ship.Data.Configurations
{
    public class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
    {
        public void Configure(EntityTypeBuilder<Currency> builder)
        {
            builder.ToTable("Currencies");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.CurrencyCode).IsRequired().HasMaxLength(3);
            builder.Property(x => x.CurrencyName).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Symbol).HasMaxLength(5);

            builder.HasIndex(x => x.CurrencyCode).IsUnique();
            builder.HasIndex(x => x.IsBaseCurrency)
                   .HasFilter("\"IsBaseCurrency\" = 1")
                   .IsUnique(); // single base currency at DB level
        }
    }
}