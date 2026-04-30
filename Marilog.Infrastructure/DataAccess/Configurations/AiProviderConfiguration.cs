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

            // Key (assuming Entity base class defines Id)
            builder.HasKey(x => x.Id);

            // Name
            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            // Provider type
            builder.Property(x => x.ProviderType)
                .IsRequired();

            // Base URL
            builder.Property(x => x.BaseUrl)
                .IsRequired()
                .HasMaxLength(500);

            // Model
            builder.Property(x => x.ModelIdentifier)
                .IsRequired()
                .HasMaxLength(200);

            // API Version (optional)
            builder.Property(x => x.ApiVersion)
                .HasMaxLength(50);

            // Tokens
            builder.Property(x => x.MaxInputTokens)
                .IsRequired();

            builder.Property(x => x.MaxOutputTokens)
                .IsRequired();

            builder.Property(x => x.ChunkOverlapTokens)
                .IsRequired();

            // Temperature
            builder.Property(x => x.DefaultTemperature)
                .IsRequired()
                .HasPrecision(5, 2);

            // API Key name
            builder.Property(x => x.ApiKeyName)
                .IsRequired()
                .HasMaxLength(200);

            // Encrypted key (important: large field)
            builder.Property(x => x.ApiKeyEncrypted)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            // Streaming support
            builder.Property(x => x.SupportsStreaming)
                .IsRequired();

            // Request template (JSON)
            builder.Property(x => x.RequestTemplateJson)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            // Indexes (important for selection performance)
            builder.HasIndex(x => x.ProviderType);

            builder.HasIndex(x => x.IsActive); // if exists in Entity base

            //builder.HasIndex(x => x.Priority);  // if exists in Entity base
        }
    }
}
