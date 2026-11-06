using Marilog.Domain.Entities.SystemEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class VesselTypeConfiguration : IEntityTypeConfiguration<VesselType>
    {
        public void Configure(EntityTypeBuilder<VesselType> builder)
        {
            builder.ToTable("VesselTypes");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).UseIdentityColumn();
            builder.Property(x => x.Code).IsRequired().HasMaxLength(30);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
            builder.Property(x => x.SortOrder).IsRequired().HasDefaultValue(0);
            builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

            builder.HasIndex(x => x.Code).IsUnique();
        }
    }
}