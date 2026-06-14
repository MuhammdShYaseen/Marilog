using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.ValueObjects.ReusableValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class CompanyConfiguration : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            builder.ToTable("Companies");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).UseIdentityColumn();
            builder.Property(x => x.CompanyName).IsRequired().HasMaxLength(200);
            builder.Property(x => x.ContactName).HasMaxLength(150);
            builder.Property(x => x.Address).HasMaxLength(300);
            builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");
            // ── Bank Accounts ─────────────────────────────────────────────
            builder.OwnsMany(c => c.BankAccounts, ba =>
            {
                ba.WithOwner().HasForeignKey("CompanyId");
                ba.ToTable("CompanyBankAccounts");

                ba.Property(b => b.IBAN)
                  .HasMaxLength(34)
                  .IsRequired();

                ba.Property(b => b.BankName)
                  .HasMaxLength(100)
                  .IsRequired();

                ba.Property(b => b.SwiftCode)
                  .HasMaxLength(11);

                ba.Property(b => b.AccountHolderName)
                  .HasMaxLength(150);

                ba.HasOne<Currency>()
                  .WithMany()
                  .HasForeignKey(b => b.CurrencyId)
                  .OnDelete(DeleteBehavior.Restrict);

                ba.HasIndex("CompanyId", nameof(BankAccount.IBAN))
                  .IsUnique();
            });

            // ── Emails ────────────────────────────────────────────────────
            builder.OwnsMany(c => c.Emails, e =>
            {
                e.WithOwner().HasForeignKey("CompanyId");
                e.ToTable("CompanyEmails");

                e.Property(x => x.Address)
                 .HasMaxLength(200)
                 .IsRequired();

                e.Property(x => x.Label)
                 .HasMaxLength(100);

                e.Property(x => x.Role)
                 .HasConversion<string>()
                 .HasMaxLength(20);

                e.HasIndex("CompanyId", nameof(ContactEmail.Address))
                 .IsUnique();
            });

            // ── Phones ────────────────────────────────────────────────────
            builder.OwnsMany(c => c.Phones, p =>
            {
                p.WithOwner().HasForeignKey("CompanyId");
                p.ToTable("CompanyPhones");

                p.Property(x => x.Number)
                 .HasMaxLength(30)
                 .IsRequired();

                p.Property(x => x.Label)
                 .HasMaxLength(100);

                p.Property(x => x.Type)
                 .HasConversion<string>()
                 .HasMaxLength(20);

                p.HasIndex("CompanyId", nameof(ContactPhone.Number), nameof(ContactPhone.Type))
                 .IsUnique();
            });
        
        builder.HasMany(x => x.Vessels)
                   .WithOne(x => x.Company)
                   .HasForeignKey(x => x.CompanyID)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Country);
        }
    }
}
