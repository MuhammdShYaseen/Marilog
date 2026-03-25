using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Marilog.Domain.Entities;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class PortConfiguration : IEntityTypeConfiguration<Port>
    {
        public void Configure(EntityTypeBuilder<Port> builder)
        {
            builder.ToTable("Ports");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).UseIdentityColumn();
            builder.Property(x => x.PortCode).IsRequired().HasMaxLength(10);
            builder.Property(x => x.PortName).IsRequired().HasMaxLength(150);
            builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");
            builder.HasIndex(x => x.PortCode).IsUnique();

            builder.HasOne(x => x.Country)
                   .WithMany()
                   .HasForeignKey(x => x.CountryID)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.CountryID);
        }
    }
}
