using Marilog.Domain.Entities.SystemEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Ship.Data.Configurations
{
    public class CrewContractConfiguration : IEntityTypeConfiguration<CrewContract>
    {
        public void Configure(EntityTypeBuilder<CrewContract> builder)
        {
            builder.ToTable("CrewContracts");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Notes).HasMaxLength(500);
            builder.Property(x => x.DurationInMonth).IsRequired().HasDefaultValue(6);
            builder.Property(x => x.WagesPaidBy)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasMaxLength(20);

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