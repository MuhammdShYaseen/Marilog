using Marilog.Domain.Entities.SystemEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Ship.Data.Configurations
{
    public class PersonConfiguration : IEntityTypeConfiguration<Person>
    {
        public void Configure(EntityTypeBuilder<Person> builder)
        {
            builder.ToTable("Persons");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.FullName).IsRequired().HasMaxLength(200);
            builder.Property(x => x.PassportNo).HasMaxLength(50);
            builder.Property(x => x.SeamanBookNo).HasMaxLength(50);
            builder.Property(x => x.Phone).HasMaxLength(50);
            builder.Property(x => x.Email).HasMaxLength(150);

            // ── Bank Account ──────────────────────────────────────────────
            builder.Property(x => x.BankName).HasMaxLength(200);
            builder.Property(x => x.IBAN).HasMaxLength(34);
            builder.Property(x => x.BankSwiftCode).HasMaxLength(11);

            // ── Certificates ──────────────────────────────────────────────
            builder.HasMany(p => p.Certificates)
                   .WithOne()
                   .HasForeignKey("PersonId")
                   .OnDelete(DeleteBehavior.Cascade);

            // ── Sea services (value object, no navigation) ────────────────
            builder.OwnsMany(p => p.SeaServices, svc =>
            {
                svc.ToTable("PersonSeaServices");
                svc.WithOwner().HasForeignKey("PersonId");
                svc.Property(s => s.RankId).IsRequired();
                svc.Property(s => s.ExperienceInMonths).IsRequired();
            });

            builder.HasOne(x => x.NationalityCountry)
                   .WithMany()
                   .HasForeignKey(x => x.Nationality)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Contracts)
                   .WithOne(x => x.Person)
                   .HasForeignKey(x => x.PersonID)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.Nationality);
        }
    }
}