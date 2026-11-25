using Marilog.Domain.Entities.SystemEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Ship.Data.Configurations
{
    public class CountryConfiguration : IEntityTypeConfiguration<Country>
    {
        public void Configure(EntityTypeBuilder<Country> builder)
        {
            builder.ToTable("Countries");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.CountryCode).IsRequired().HasMaxLength(3);
            builder.Property(x => x.CountryName).IsRequired().HasMaxLength(100);

            builder.HasIndex(x => x.CountryCode).IsUnique();
        }
    }
}