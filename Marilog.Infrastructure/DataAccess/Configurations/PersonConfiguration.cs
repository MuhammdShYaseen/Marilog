
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Marilog.Domain.Entities;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class PersonConfiguration : IEntityTypeConfiguration<Person>
    {
        public void Configure(EntityTypeBuilder<Person> builder)
        {
            builder.ToTable("Persons");
            builder.HasKey(x => x.PersonID);
            builder.Property(x => x.PersonID).UseIdentityColumn();
            builder.Property(x => x.FullName).IsRequired().HasMaxLength(200);
            builder.Property(x => x.PassportNo).HasMaxLength(50);
            builder.Property(x => x.PassportExpiry).HasColumnType("date");
            builder.Property(x => x.SeamanBookNo).HasMaxLength(50);
            builder.Property(x => x.DateOfBirth).HasColumnType("date");
            builder.Property(x => x.Phone).HasMaxLength(50);
            builder.Property(x => x.Email).HasMaxLength(150);
            builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

            builder.HasOne(x => x.NationalityCountry)
                   .WithMany()
                   .HasForeignKey(x => x.Nationality)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(x => x.Contracts)
                   .WithOne(x => x.Person)
                   .HasForeignKey(x => x.PersonID)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.Nationality);
        }
    }
}
