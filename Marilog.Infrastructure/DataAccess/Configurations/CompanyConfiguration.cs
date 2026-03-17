using Marilog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class CompanyConfiguration : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            builder.ToTable("Companies");
            builder.HasKey(x => x.CompanyID);
            builder.Property(x => x.CompanyID).UseIdentityColumn();
            builder.Property(x => x.CompanyName).IsRequired().HasMaxLength(200);
            builder.Property(x => x.ContactName).HasMaxLength(150);
            builder.Property(x => x.Email).HasMaxLength(150);
            builder.Property(x => x.Phone).HasMaxLength(50);
            builder.Property(x => x.Address).HasMaxLength(300);
            builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

            builder.HasMany(x => x.Vessels)
                   .WithOne(x => x.Company)
                   .HasForeignKey(x => x.CompanyID)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Country);
        }
    }
}
