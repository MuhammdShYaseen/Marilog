using Marilog.Domain.Entities.SystemEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Ship.Data.Configurations
{
    public class RankConfiguration : IEntityTypeConfiguration<Rank>
    {
        public void Configure(EntityTypeBuilder<Rank> builder)
        {
            builder.ToTable("Ranks", t => t.HasCheckConstraint(
                "CK_Ranks_Department",
                "\"Department\" IN ('DECK', 'ENGINE', 'CATERING')"));
            builder.HasKey(x => x.Id);
            builder.Property(x => x.RankCode).IsRequired().HasMaxLength(20);
            builder.Property(x => x.RankName).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Department)
                   .IsRequired()
                   .HasMaxLength(20)
                   .HasConversion<string>();

            builder.HasIndex(x => x.RankCode).IsUnique();
        }
    }
}