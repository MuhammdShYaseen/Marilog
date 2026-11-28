using Marilog.Ship.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Ship.Data.Configurations
{
    public class MgaCrewLineConfiguration : IEntityTypeConfiguration<MgaCrewLine>
    {
        public void Configure(EntityTypeBuilder<MgaCrewLine> builder)
        {
            builder.ToTable("MgaCrewLines");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.WagesPaidBy)
                    .HasConversion<string>()
                    .HasMaxLength(20);
            builder.HasOne(x => x.CrewContract)
                   .WithMany()
                   .HasForeignKey(x => x.CrewContractId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.MgaId, x.CrewContractId }).IsUnique();
        }
    }
}