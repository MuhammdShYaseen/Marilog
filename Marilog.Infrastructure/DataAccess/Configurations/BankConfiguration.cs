using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.ValueObjects.ReusableValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace Marilog.Infrastructure.DataAccess.Configurations
{
    internal sealed class BankConfiguration : IEntityTypeConfiguration<Bank>
    {
        public void Configure(EntityTypeBuilder<Bank> builder)
        {
            builder.ToTable("Banks");

            builder.HasKey(x => x.Id);

            //==========================================================
            // Properties
            //==========================================================

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.ShortName)
                .HasMaxLength(100);

            builder.Property(x => x.LegalName)
                .HasMaxLength(250);

            builder.Property(x => x.SwiftBic)
                .HasMaxLength(11);

            builder.Property(x => x.BranchCode)
                .HasMaxLength(50);

            builder.Property(x => x.ClearingCode)
                .HasMaxLength(50);

            builder.Property(x => x.NationalBankCode)
                .HasMaxLength(50);

            builder.Property(x => x.City)
                .HasMaxLength(150);

            builder.Property(x => x.Address)
                .HasMaxLength(500);

            builder.Property(x => x.Website)
                .HasMaxLength(300);

            builder.Property(x => x.Notes)
                .HasMaxLength(2000);

            //==========================================================
            // Relationships
            //==========================================================

            builder.HasOne(x => x.Country)
                .WithMany()
                .HasForeignKey(x => x.CountryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ParentBank)
                .WithMany(x => x.Branches)
                .HasForeignKey(x => x.ParentBankId)
                .OnDelete(DeleteBehavior.Restrict);

            //==========================================================
            // Owned Collection : Emails
            //==========================================================

            builder.OwnsMany(x => x.Emails, email =>
            {
                email.ToTable("BankEmails");

                email.WithOwner().HasForeignKey("BankId");

                email.HasKey("Id");

                email.Property<int>("Id")
                     .ValueGeneratedOnAdd();

                email.Property(e => e.Address)
                     .IsRequired()
                     .HasMaxLength(320);

                email.Property(e => e.Role)
                     .HasConversion<int>();

                email.Property(e => e.Label)
                     .HasMaxLength(100);

                email.Property(e => e.IsPrimary)
                     .IsRequired();

                email.HasIndex("BankId", nameof(ContactEmail.Address))
                     .IsUnique();
            });

            //==========================================================
            // Owned Collection : Phones
            //==========================================================

            builder.OwnsMany(x => x.Phones, phone =>
            {
                phone.ToTable("BankPhones");

                phone.WithOwner().HasForeignKey("BankId");

                phone.HasKey("Id");

                phone.Property<int>("Id")
                     .ValueGeneratedOnAdd();

                phone.Property(p => p.Number)
                     .IsRequired()
                     .HasMaxLength(30);

                phone.Property(p => p.Label)
                     .HasMaxLength(100);

                phone.Property(p => p.Type)
                     .HasConversion<int>();

                phone.Property(p => p.IsPrimary)
                     .IsRequired();

                phone.HasIndex("BankId", nameof(ContactPhone.Number), nameof(ContactPhone.Type))
                     .IsUnique();
            });

            //==========================================================
            // Indexes
            //==========================================================

            builder.HasIndex(x => x.Name);

            builder.HasIndex(x => x.CountryId);

            builder.HasIndex(x => x.ParentBankId);

            builder.HasIndex(x => x.SwiftBic)
                .IsUnique()
                .HasFilter("[SwiftBic] IS NOT NULL");

            builder.HasIndex(x => new
            {
                x.ParentBankId,
                x.Name
            });
        }
    }
}
