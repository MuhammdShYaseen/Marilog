using Marilog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class SwiftTransferConfiguration : IEntityTypeConfiguration<SwiftTransfer>
    {
        public void Configure(EntityTypeBuilder<SwiftTransfer> builder)
        {
            builder.ToTable("swift_transfers");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).UseIdentityColumn();
            builder.Property(x => x.SwiftReference).IsRequired().HasMaxLength(50);
            builder.Property(x => x.TransactionDate).IsRequired().HasColumnType("date");
            builder.Property(x => x.CurrencyId).IsRequired();
            builder.Property(x => x.Amount).IsRequired().HasColumnType("decimal(18,2)");
            builder.Property(x => x.SenderBank).HasMaxLength(200);
            builder.Property(x => x.ReceiverBank).HasMaxLength(200);
            builder.Property(x => x.PaymentReference).HasMaxLength(200);
            builder.Property(x => x.RawMessage).HasColumnType("nvarchar(max)");
            builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

            builder.HasOne(x => x.Currency)
                   .WithMany()
                   .HasForeignKey(x => x.CurrencyId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.SenderCompany)
                   .WithMany()
                   .HasForeignKey(x => x.SenderCompanyId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.ReceiverCompany)
                   .WithMany()
                   .HasForeignKey(x => x.ReceiverCompanyId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(x => x.Payments)
                   .WithOne(x => x.SwiftTransfer)
                   .HasForeignKey(x => x.SwiftTransferId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.SwiftReference).IsUnique();
            builder.HasIndex(x => x.SenderCompanyId);
            builder.HasIndex(x => x.ReceiverCompanyId);
            builder.HasIndex(x => x.TransactionDate);
            builder.HasIndex(x => x.CurrencyId);
        }
    }
}
