using Marilog.Domain.Entities.SystemEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
    {
        public void Configure(EntityTypeBuilder<Currency> builder)
        {
            builder.ToTable("Currencies");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).UseIdentityColumn();
            builder.Property(x => x.CurrencyCode).IsRequired().HasMaxLength(3);
            builder.Property(x => x.CurrencyName).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Symbol).HasMaxLength(5);
            builder.Property(x => x.ExchangeRate).IsRequired().HasColumnType("decimal(18,6)");
            builder.Property(x => x.IsBaseCurrency).IsRequired().HasDefaultValue(false);
            builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

            builder.HasIndex(x => x.CurrencyCode).IsUnique();
            builder.HasIndex(x => x.IsBaseCurrency)
                   .HasFilter("[IsBaseCurrency] = 1")
                   .IsUnique(); // enforce single base currency at DB level
        }
    }
}
