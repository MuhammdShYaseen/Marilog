using Marilog.Application.DTOs.Ai;
using Marilog.Application.Interfaces.Ai;
using Marilog.Application.Interfaces.Encryption;
using Marilog.Contracts.DTOs.Requests.AiProviderDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.AiProviderServices;
using Marilog.Domain.Entities.AiEntities;
using Marilog.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Marilog.Application.Services.ApplicationServices.AiServices
{
    public sealed class AiProviderService : IAiProviderService
    {
        private readonly IRepository<AiProvider> _repo;
        private readonly ISecretEncryptionService _encryption;
        private readonly IAiCompletionService _completion;

        public AiProviderService(
            IRepository<AiProvider> repo,
            ISecretEncryptionService encryption,
            IAiCompletionService completion)
        {
            _repo = repo;
            _encryption = encryption;
            _completion = completion;
        }

        public async Task<IEnumerable<AiProviderResponse>> GetAllAsync(CancellationToken ct)
        {
            return await _repo.Query()
                .AsNoTracking()
                .OrderBy(p => p.Purpose).ThenBy(p => p.Name)
                .Select(ToResponse)
                .ToListAsync(ct);
        }

        public async Task<AiProviderResponse?> GetByIdAsync(int id, CancellationToken ct)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(ToResponse)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<AiProviderResponse> CreateAsync(CreateAiProviderRequest request, CancellationToken ct)
        {
            var entity = AiProvider.Create(
                request.Name,
                request.ProviderType,
                request.Purpose,
                request.BaseUrl,
                request.ModelIdentifier,
                request.AuthHeaderName,
                request.AuthPrefix,
                request.ExtraHeadersJson,
                request.RequestTemplateJson,
                request.ResponseTextPath,
                request.MaxInputTokens,
                request.MaxOutputTokens,
                request.ChunkOverlapTokens,
                request.DefaultTemperature);

            SetApiKey(entity, request.ApiKey);

            await _repo.AddAsync(entity, ct);
            await _repo.SaveChangesAsync(ct);

            return ToResponseFunc(entity);
        }

        public async Task<bool> UpdateAsync(int id, UpdateAiProviderRequest request, CancellationToken ct)
        {
            var entity = await _repo.GetByIdAsync(id, ct);
            if (entity is null) return false;

            entity.Update(
                request.Name,
                request.ProviderType,
                request.BaseUrl,
                request.ModelIdentifier,
                request.AuthHeaderName,
                request.AuthPrefix,
                request.ExtraHeadersJson,
                request.RequestTemplateJson,
                request.ResponseTextPath,
                request.MaxInputTokens,
                request.MaxOutputTokens,
                request.ChunkOverlapTokens,
                request.DefaultTemperature);

            SetApiKey(entity, request.ApiKey);

            _repo.Update(entity);
            await _repo.SaveChangesAsync(ct);

            return true;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct)
        {
            var entity = await _repo.GetByIdAsync(id, ct);
            if (entity is null) return false;

            _repo.HardDelete(entity);
            await _repo.SaveChangesAsync(ct);

            return true;
        }

        // One active provider per purpose: activating this one deactivates the current one.
        public async Task<bool> ActivateAsync(int id, CancellationToken ct)
        {
            var entity = await _repo.GetByIdAsync(id, ct);
            if (entity is null) return false;
            if (entity.IsActive) return true;

            var purpose = entity.Purpose;
            var current = await _repo.Query()
                .Where(p => p.Purpose == purpose && p.IsActive)
                .ToListAsync(ct);

            foreach (var other in current)
            {
                other.Deactivate();
                _repo.Update(other);
            }

            entity.Activate();
            _repo.Update(entity);
            await _repo.SaveChangesAsync(ct);

            return true;
        }

        public async Task<bool> DeactivateAsync(int id, CancellationToken ct)
        {
            var entity = await _repo.GetByIdAsync(id, ct);
            if (entity is null) return false;
            if (!entity.IsActive) return true;

            entity.Deactivate();
            _repo.Update(entity);
            await _repo.SaveChangesAsync(ct);

            return true;
        }

        public Task<string> TestAsync(int id, string prompt, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                throw new ArgumentException("Prompt is required.", nameof(prompt));

            return _completion.CompleteAsync(id, new AiRequestContext { Prompt = prompt }, ct);
        }

        //----Privates---------------------------------------------------------

        // Empty key = keep the stored one. Only the cipher text and the last 4 characters are stored.
        private void SetApiKey(AiProvider entity, string? apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey)) return;

            var key = apiKey.Trim();
            entity.SetApiKey(_encryption.Encrypt(key), key.Length >= 12 ? key[^4..] : null);
        }

        private static readonly Expression<Func<AiProvider, AiProviderResponse>> ToResponse = e => new AiProviderResponse
        {
            Id = e.Id,
            Name = e.Name,
            ProviderType = e.ProviderType,
            Purpose = e.Purpose,
            BaseUrl = e.BaseUrl,
            ModelIdentifier = e.ModelIdentifier,
            AuthHeaderName = e.AuthHeaderName,
            AuthPrefix = e.AuthPrefix,
            HasApiKey = e.ApiKeyEncrypted != null,
            ApiKeyHint = e.ApiKeyHint,
            ExtraHeadersJson = e.ExtraHeadersJson,
            RequestTemplateJson = e.RequestTemplateJson,
            ResponseTextPath = e.ResponseTextPath,
            MaxInputTokens = e.MaxInputTokens,
            MaxOutputTokens = e.MaxOutputTokens,
            ChunkOverlapTokens = e.ChunkOverlapTokens,
            DefaultTemperature = e.DefaultTemperature,
            IsActive = e.IsActive
        };

        // Declared after ToResponse: static fields initialise in order.
        private static readonly Func<AiProvider, AiProviderResponse> ToResponseFunc = ToResponse.Compile();
    }
}