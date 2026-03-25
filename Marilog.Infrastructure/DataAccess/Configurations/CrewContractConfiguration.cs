using Marilog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class CrewContractConfiguration : IEntityTypeConfiguration<CrewContract>
    {
        public void Configure(EntityTypeBuilder<CrewContract> builder)
        {
            builder.ToTable("CrewContracts");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).UseIdentityColumn();
            builder.Property(x => x.MonthlyWage).IsRequired().HasColumnType("decimal(10,2)");
            builder.Property(x => x.SignOnDate).IsRequired().HasColumnType("date");
            builder.Property(x => x.SignOffDate).HasColumnType("date");
            builder.Property(x => x.Notes).HasMaxLength(500);
            builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

            builder.HasOne(x => x.Rank)
                   .WithMany()
                   .HasForeignKey(x => x.RankID)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.SignOnPortNav)
                   .WithMany()
                   .HasForeignKey(x => x.SignOnPort)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.SignOffPortNav)
                   .WithMany()
                   .HasForeignKey(x => x.SignOffPort)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.PersonID);
            builder.HasIndex(x => x.VesselID);
            builder.HasIndex(x => new { x.IsActive, x.SignOffDate });
        }
    }
}
