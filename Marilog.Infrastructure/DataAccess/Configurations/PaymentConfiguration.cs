using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Marilog.Domain.Entities.SystemEntities;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("payments");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).UseIdentityColumn();
            builder.Property(x => x.PaidAmount).IsRequired().HasColumnType("decimal(18,2)");
            builder.Property(x => x.PaymentDate).IsRequired().HasColumnType("date");

            // ── Matching query filter to avoid global filter warning ─────────
            builder.HasQueryFilter(x => !x.SwiftTransfer!.IsDeleted);

            builder.HasIndex(x => x.DocumentId);
            builder.HasIndex(x => x.SwiftTransferId);
        }
    }
}

