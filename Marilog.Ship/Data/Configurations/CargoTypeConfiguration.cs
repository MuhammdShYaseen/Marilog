using Marilog.Domain.Entities.SystemEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Ship.Data.Configurations
{
    public class CargoTypeConfiguration : IEntityTypeConfiguration<CargoType>
    {
        public void Configure(EntityTypeBuilder<CargoType> builder)
        {
            builder.ToTable("CargoTypes");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Code).IsRequired().HasMaxLength(30);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Category).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.DefaultUnit).IsRequired().HasConversion<string>().HasMaxLength(20);

            builder.HasIndex(x => x.Code).IsUnique();
        }
    }
}