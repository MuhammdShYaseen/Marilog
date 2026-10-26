using Marilog.Domain.Entities.SystemEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class SwiftBankFeeConfiguration : IEntityTypeConfiguration<SwiftBankFee>
    {
        public void Configure(EntityTypeBuilder<SwiftBankFee> builder)
        {
            builder.ToTable("swift_bank_fees");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).UseIdentityColumn();
            builder.Property(x => x.Stage).IsRequired();
            builder.Property(x => x.Amount).IsRequired().HasColumnType("decimal(18,2)");
            builder.Property(x => x.IsDeducted).IsRequired();
            builder.Property(x => x.FeeDate).IsRequired().HasColumnType("date");
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

            builder.HasOne(x => x.Bank)
                   .WithMany()
                   .HasForeignKey(x => x.BankId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.SwiftTransferId);
            builder.HasIndex(x => x.BankId);
        }
    }
}