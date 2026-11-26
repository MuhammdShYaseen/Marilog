using Marilog.Ship.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Ship.Data.Configurations
{
    public class DeductionTypeConfiguration : IEntityTypeConfiguration<DeductionType>
    {
        public void Configure(EntityTypeBuilder<DeductionType> builder)
        {
            builder.ToTable("DeductionTypes");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Code).IsRequired().HasMaxLength(30);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(100);

            builder.HasIndex(x => x.Code).IsUnique();
        }
    }
}