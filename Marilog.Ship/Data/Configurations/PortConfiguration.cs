using Marilog.Domain.Entities.SystemEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Ship.Data.Configurations
{
    public class PortConfiguration : IEntityTypeConfiguration<Port>
    {
        public void Configure(EntityTypeBuilder<Port> builder)
        {
            builder.ToTable("Ports");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.PortCode).IsRequired().HasMaxLength(10);
            builder.Property(x => x.PortName).IsRequired().HasMaxLength(150);

            builder.HasIndex(x => x.PortCode).IsUnique();
            builder.HasIndex(x => x.CountryID);

            builder.HasOne(x => x.Country)
                   .WithMany()
                   .HasForeignKey(x => x.CountryID)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}