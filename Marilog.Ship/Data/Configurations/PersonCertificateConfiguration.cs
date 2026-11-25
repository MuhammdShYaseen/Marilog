using Marilog.Domain.Entities.Certificates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Ship.Data.Configurations
{
    public class PersonCertificateConfiguration : IEntityTypeConfiguration<PersonCertificate>
    {
        public void Configure(EntityTypeBuilder<PersonCertificate> builder)
        {
            builder.ToTable("PersonCertificates");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Type)
                   .HasConversion<string>()
                   .HasMaxLength(100)
                   .IsRequired();

            builder.OwnsOne(c => c.Certificate, cert =>
            {
                cert.Property(v => v.CertificateName).HasMaxLength(200).IsRequired();
                cert.Property(v => v.CertificateNumber).HasMaxLength(200);
                cert.Property(v => v.IssuingAuthority).HasMaxLength(200);
                cert.Property(v => v.Description).HasMaxLength(1000);
            });
        }
    }
}