using Marilog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class CountryConfiguration : IEntityTypeConfiguration<Country>
    {
        public void Configure(EntityTypeBuilder<Country> builder)
        {
            builder.ToTable("Countries");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).UseIdentityColumn();
            builder.Property(x => x.CountryCode).IsRequired().HasMaxLength(3);
            builder.Property(x => x.CountryName).IsRequired().HasMaxLength(100);
            builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");
            builder.HasIndex(x => x.CountryCode).IsUnique();
        }
    }
}
