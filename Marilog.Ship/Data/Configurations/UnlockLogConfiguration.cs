using Marilog.Ship.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Ship.Data.Configurations
{
    public class UnlockLogConfiguration : IEntityTypeConfiguration<UnlockLog>
    {
        public void Configure(EntityTypeBuilder<UnlockLog> builder)
        {
            builder.ToTable("UnlockLogs");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Target)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasMaxLength(20);
            builder.Property(x => x.RequestCode).IsRequired().HasMaxLength(6);
            builder.Property(x => x.Reason).IsRequired().HasMaxLength(500);

            builder.HasIndex(x => new { x.Target, x.TargetId });
        }
    }
}