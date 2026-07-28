using Marilog.Domain.Entities.SystemEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class EmailAccountConfiguration : IEntityTypeConfiguration<EmailAccount>
    {
        public void Configure(EntityTypeBuilder<EmailAccount> builder)
        {
            builder.ToTable("EmailAccounts");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).UseIdentityColumn();

            builder.Property(x => x.DisplayName).IsRequired().HasMaxLength(150);
            builder.Property(x => x.EmailAddress).IsRequired().HasMaxLength(320);

            builder.Property(x => x.ProviderType)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasMaxLength(30);

            builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);

            builder.Property(x => x.EncryptedConfig)
                   .IsRequired()
                   .HasColumnType("nvarchar(max)");

            builder.Property(x => x.LastSyncedAt);
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

            // One EmailAccount can be the sender/receiver for many Email records.
            // FK lives on Email (EmailAccountId) — configured in EmailConfiguration.
            builder.HasIndex(x => x.EmailAddress).IsUnique();
            builder.HasIndex(x => x.ProviderType);
            builder.HasIndex(x => x.IsActive);
        }
    }
}