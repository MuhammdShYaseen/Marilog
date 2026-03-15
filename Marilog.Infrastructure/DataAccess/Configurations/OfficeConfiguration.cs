using Marilog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class OfficeConfiguration : IEntityTypeConfiguration<Office>
    {
        public void Configure(EntityTypeBuilder<Office> builder)
        {
            builder.ToTable("Offices");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).UseIdentityColumn();
            builder.Property(x => x.OfficeName).IsRequired().HasMaxLength(150);
            builder.Property(x => x.City).IsRequired().HasMaxLength(100);
            builder.Property(x => x.CountryId).IsRequired();
            builder.Property(x => x.Address).HasMaxLength(300);
            builder.Property(x => x.Phone).HasMaxLength(50);
            builder.Property(x => x.ContactName).HasMaxLength(150);
            builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

            builder.HasOne(x => x.Country)
                   .WithMany()
                   .HasForeignKey(x => x.CountryId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.CountryId);
        }
    }
}
