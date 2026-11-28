using Marilog.Domain.Entities.SystemEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Ship.Data.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("payments");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Discreption).HasMaxLength(200);
            builder.Property(x => x.PaymentMethod).IsRequired();

            // No SWIFT transfers on board (SwiftTransfer is ignored in ShipContext):
            // SwiftTransferId stays as a column, always null.

            builder.HasIndex(x => x.DocumentId);
        }
    }
}