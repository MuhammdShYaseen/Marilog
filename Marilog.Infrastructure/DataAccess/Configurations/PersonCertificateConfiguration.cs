using Marilog.Domain.Entities.Certificates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Infrastructure.DataAccess.Configurations
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
                cert.Property(v => v.IssueDate);
                cert.Property(v => v.ExpiryDate);
            });
        }
    }
}
