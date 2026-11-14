using Marilog.Domain.Entities.AiEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public sealed class AiProviderConfiguration : IEntityTypeConfiguration<AiProvider>
    {
        public void Configure(EntityTypeBuilder<AiProvider> builder)
        {
            builder.ToTable("AiProviders");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.ProviderType).IsRequired();
            builder.Property(x => x.Purpose).IsRequired();

            builder.Property(x => x.BaseUrl).IsRequired().HasMaxLength(500);
            builder.Property(x => x.ModelIdentifier).IsRequired().HasMaxLength(200);

            builder.Property(x => x.AuthHeaderName).IsRequired().HasMaxLength(100);
            builder.Property(x => x.AuthPrefix).IsRequired().HasMaxLength(50);

            builder.Property(x => x.ApiKeyEncrypted).HasColumnType("nvarchar(max)");
            builder.Property(x => x.ApiKeyHint).HasMaxLength(10);

            builder.Property(x => x.ExtraHeadersJson).HasColumnType("nvarchar(max)");
            builder.Property(x => x.RequestTemplateJson).IsRequired().HasColumnType("nvarchar(max)");
            builder.Property(x => x.ResponseTextPath).IsRequired().HasMaxLength(200);

            builder.Property(x => x.MaxInputTokens).IsRequired();
            builder.Property(x => x.MaxOutputTokens).IsRequired();
            builder.Property(x => x.ChunkOverlapTokens).IsRequired();
            builder.Property(x => x.DefaultTemperature).IsRequired().HasPrecision(5, 2);

            builder.HasIndex(x => new { x.Purpose, x.IsActive });
        }
    }
}