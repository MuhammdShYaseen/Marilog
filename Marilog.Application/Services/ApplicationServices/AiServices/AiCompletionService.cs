using Marilog.Application.DTOs.Ai;
using Marilog.Application.Interfaces.Ai;
using Marilog.Application.Interfaces.Encryption;
using Marilog.Domain.Entities.AiEntities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Kernel.Enums;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace Marilog.Application.Services.ApplicationServices.AiServices
{
    public sealed class AiCompletionService : IAiCompletionService
    {
        private readonly IRepository<AiProvider> _repo;
        private readonly ISecretEncryptionService _encryption;
        private readonly IAiRequestBuilder _builder;
        private readonly IAiProviderClient _client;

        public AiCompletionService(
            IRepository<AiProvider> repo,
            ISecretEncryptionService encryption,
            IAiRequestBuilder builder,
            IAiProviderClient client)
        {
            _repo = repo;
            _encryption = encryption;
            _builder = builder;
            _client = client;
        }

        public async Task<string> CompleteAsync(AiProviderPurpose purpose, AiRequestContext context, CancellationToken ct)
        {
            var provider = await GetActiveAsync(purpose, ct);
            return await _client.SendAsync(provider, DecryptKey(provider), _builder.Build(provider, context), ct);
        }

        public async Task<string> CompleteAsync(int providerId, AiRequestContext context, CancellationToken ct)
        {
            var provider = await GetByIdAsync(providerId, ct);
            return await _client.SendAsync(provider, DecryptKey(provider), _builder.Build(provider, context), ct);
        }

        public async IAsyncEnumerable<string> StreamAsync(
            AiProviderPurpose purpose, AiRequestContext context, [EnumeratorCancellation] CancellationToken ct)
        {
            var provider = await GetActiveAsync(purpose, ct);

            await foreach (var text in Stream(provider, context, ct))
                yield return text;
        }

        public async IAsyncEnumerable<string> StreamAsync(
            int providerId, AiRequestContext context, [EnumeratorCancellation] CancellationToken ct)
        {
            var provider = await GetByIdAsync(providerId, ct);

            await foreach (var text in Stream(provider, context, ct))
                yield return text;
        }

        //----Privates---------------------------------------------------------

        private IAsyncEnumerable<string> Stream(AiProvider provider, AiRequestContext context, CancellationToken ct)
        {
            if (provider.StreamTextPath is null)
                throw new InvalidOperationException($"AI provider '{provider.Name}' has no StreamTextPath, so it can't stream.");

            return _client.StreamAsync(provider, DecryptKey(provider), _builder.Build(provider, context, stream: true), ct);
        }

        private async Task<AiProvider> GetActiveAsync(AiProviderPurpose purpose, CancellationToken ct)
            => await _repo.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Purpose == purpose && p.IsActive, ct)
                ?? throw new InvalidOperationException($"No active AI provider is set for {purpose}.");

        private async Task<AiProvider> GetByIdAsync(int providerId, CancellationToken ct)
            => await _repo.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == providerId, ct)
                ?? throw new KeyNotFoundException($"AI provider {providerId} not found.");

        private string? DecryptKey(AiProvider provider)
            => provider.ApiKeyEncrypted is null ? null : _encryption.Decrypt(provider.ApiKeyEncrypted);
    }
}