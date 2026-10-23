using Marilog.Infrastructure.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
    {
        public void Configure(EntityTypeBuilder<RolePermission> builder)
        {
            builder.ToTable("RolePermissions");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).UseIdentityColumn();
            builder.Property(x => x.TargetKind).IsRequired();
            builder.Property(x => x.TargetValue).IsRequired();
            builder.Property(x => x.Actions).IsRequired();
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

            // One row per role and target.
            builder.HasIndex(x => new { x.RoleId, x.TargetKind, x.TargetValue })
                   .IsUnique()
                   .HasFilter("[IsDeleted] = 0");
        }
    }
}