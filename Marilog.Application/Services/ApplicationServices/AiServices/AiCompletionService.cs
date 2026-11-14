using Marilog.Application.DTOs.Ai;
using Marilog.Application.Interfaces.Ai;
using Marilog.Application.Interfaces.Encryption;
using Marilog.Domain.Entities.AiEntities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Kernel.Enums;
using Microsoft.EntityFrameworkCore;

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
            var provider = await _repo.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Purpose == purpose && p.IsActive, ct)
                ?? throw new InvalidOperationException($"No active AI provider is set for {purpose}.");

            return await SendAsync(provider, context, ct);
        }

        public async Task<string> CompleteAsync(int providerId, AiRequestContext context, CancellationToken ct)
        {
            var provider = await _repo.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == providerId, ct)
                ?? throw new KeyNotFoundException($"AI provider {providerId} not found.");

            return await SendAsync(provider, context, ct);
        }

        //----Privates---------------------------------------------------------

        private Task<string> SendAsync(AiProvider provider, AiRequestContext context, CancellationToken ct)
        {
            var apiKey = provider.ApiKeyEncrypted is null ? null : _encryption.Decrypt(provider.ApiKeyEncrypted);
            var body = _builder.Build(provider, context);

            return _client.SendAsync(provider, apiKey, body, ct);
        }
    }
}