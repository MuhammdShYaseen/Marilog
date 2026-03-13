using Marilog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class DocumentConfiguration : IEntityTypeConfiguration<Document>
    {
        public void Configure(EntityTypeBuilder<Document> builder)
        {
            builder.ToTable("documents");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).UseIdentityColumn();
            builder.Property(x => x.DocNumber).IsRequired().HasMaxLength(50);
            builder.Property(x => x.DocType).IsRequired().HasMaxLength(50);
            builder.Property(x => x.DocDate).IsRequired().HasColumnType("date");
            builder.Property(x => x.CurrencyId).IsRequired();
            builder.Property(x => x.TotalAmount).IsRequired().HasColumnType("decimal(18,2)");
            builder.HasOne(x => x.Currency)
                   .WithMany()
                   .HasForeignKey(x => x.CurrencyId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.Reference).HasMaxLength(200);
            builder.Property(x => x.FilePath).HasMaxLength(500);
            builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

            builder.HasOne(x => x.Supplier)
                   .WithMany()
                   .HasForeignKey(x => x.SupplierId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.Buyer)
                   .WithMany()
                   .HasForeignKey(x => x.BuyerId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.Vessel)
                   .WithMany()
                   .HasForeignKey(x => x.VesselId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.Port)
                   .WithMany()
                   .HasForeignKey(x => x.PortId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(x => x.Items)
                   .WithOne()
                   .HasForeignKey(x => x.DocumentId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Payments)
                   .WithOne()
                   .HasForeignKey(x => x.DocumentId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.DocNumber).IsUnique();
            builder.HasIndex(x => x.SupplierId);
            builder.HasIndex(x => x.BuyerId);
            builder.HasIndex(x => x.VesselId);
        }
    }
}
