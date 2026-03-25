using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Marilog.Domain.Entities;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class RankConfiguration : IEntityTypeConfiguration<Rank>
    {
        public void Configure(EntityTypeBuilder<Rank> builder)
        {
            builder.ToTable("Ranks");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).UseIdentityColumn();
            builder.Property(x => x.RankCode).IsRequired().HasMaxLength(20);
            builder.Property(x => x.RankName).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Department)
                   .IsRequired()
                   .HasMaxLength(20)
                   .HasConversion<string>();
            builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
            builder.HasIndex(x => x.RankCode).IsUnique();

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_Ranks_Department",
                "Department IN ('DECK', 'ENGINE', 'CATERING')"));
        }
    }
}
