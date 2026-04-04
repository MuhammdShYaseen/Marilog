// Infrastructure/Persistence/Configurations/ContractConfiguration.cs
using Marilog.Domain.Entities;
using Marilog.Domain.Enumerations;
using Marilog.Domain.ValueObjects.Contract;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Marilog.Infrastructure.Persistence.Configurations
{
    public class ContractConfiguration : IEntityTypeConfiguration<AContract>
    {
        public void Configure(EntityTypeBuilder<AContract> builder)
        {
            // ─── Table & Key ──────────────────────────────────────────────────
            builder.ToTable("Contracts");
            builder.HasKey(c => c.Id);

            // ─── Properties ───────────────────────────────────────────────────
            builder.Property(c => c.ContractNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(c => c.Type)
                .IsRequired()
                .HasConversion<string>()        // ✅ "CharterParty" في DB
                .HasMaxLength(30);

            builder.Property(c => c.Status)
                .IsRequired()
                .HasConversion(
                    new ValueConverter<ContractStatus, int>(
                        v => v.Id,
                        v => ContractStatus.FromId(v)));

            builder.Property(c => c.EffectiveDate)
                .IsRequired()
                .HasColumnType("date");

            builder.Property(c => c.ExpiryDate)
                .HasColumnType("date");

            builder.Property(c => c.Notes)
                .HasMaxLength(2000);

            builder.Property(c => c.ContractFileUrl)
                .HasMaxLength(500);

            builder.Property(c => c.ContractFileName)
                .HasMaxLength(200);

            // ─── Parties ──────────────────────────────────────────────────────
            builder.OwnsMany(c => c.Parties, b =>
            {
                b.WithOwner().HasForeignKey("ContractId");
                b.ToTable("ContractParties");
                b.HasKey("ContractId", nameof(ContractParty.CompanyId), nameof(ContractParty.Role));

                b.Property(p => p.CompanyId).IsRequired();

                b.Property(p => p.Role)
                    .IsRequired()
                    .HasConversion<string>()    // ✅ "Owner" في DB
                    .HasMaxLength(30);
            });

            builder.Navigation(c => c.Parties)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            // ─── Amendments ───────────────────────────────────────────────────
            builder.OwnsMany(c => c.Amendments, b =>        // ✅ OwnsMany لا HasMany
            {
                b.WithOwner().HasForeignKey("ContractId");
                b.ToTable("ContractAmendments");
                b.HasKey("ContractId", nameof(ContractAmendment.AmendmentNumber));  // ✅ Composite Key

                b.Property(a => a.AmendmentNumber).IsRequired();

                b.Property(a => a.Description)
                    .IsRequired()
                    .HasMaxLength(1000);

                b.Property(a => a.EffectiveDate)
                    .IsRequired()
                    .HasColumnType("date");

                b.Property(a => a.ChangedBy)
                    .IsRequired()
                    .HasMaxLength(100);

                b.Property(a => a.RecordedAtUtc)
                    .IsRequired()
                    .HasColumnType("datetime2");

                b.HasIndex("ContractId");
            });

            builder.Navigation(c => c.Amendments)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            // ─── Indexes ──────────────────────────────────────────────────────
            builder.HasIndex(c => c.ContractNumber).IsUnique();
        }
    }
}