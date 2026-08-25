
using Marilog.Domain.Entities.SystemEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
#if false
    public class CreditBalanceConfiguration : IEntityTypeConfiguration<CreditBalance>
    {
        public void Configure(EntityTypeBuilder<CreditBalance> builder)
        {
            

            builder.ToTable("CreditBalance");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.PaymentId).IsRequired();
            builder.Property(x => x.Id).UseIdentityColumn();
            builder.Property(x => x.CurrencyId).IsRequired();
            builder.Property(x => x.Amount).IsRequired().HasColumnType("decimal(18,2)");
            builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

            builder.HasOne(x => x.Currency)
                   .WithMany()
                   .HasForeignKey(x => x.CurrencyId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.SenderCompany)
                   .WithMany()
                   .HasForeignKey(x => x.SenderCompanyId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ReceiverCompany)
                   .WithMany()
                   .HasForeignKey(x => x.ReceiverCompanyId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Payments)
                   .WithOne(x => x.CreditBalance)
                   .HasForeignKey(x => x.CreditBalanceId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.SenderCompanyId);
            builder.HasIndex(x => x.ReceiverCompanyId);
            builder.HasIndex(x => x.CurrencyId);
            builder.HasIndex(x => x.PaymentId);


        }
    }
#endif
}
