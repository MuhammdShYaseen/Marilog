using Marilog.Ship.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Ship.Data.Configurations
{
    public class MgaCashLineConfiguration : IEntityTypeConfiguration<MgaCashLine>
    {
        public void Configure(EntityTypeBuilder<MgaCashLine> builder)
        {
            builder.ToTable("MgaCashLines");
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Currency)
                   .WithMany()
                   .HasForeignKey(x => x.CurrencyId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.MgaId, x.CurrencyId }).IsUnique();
        }
    }
}