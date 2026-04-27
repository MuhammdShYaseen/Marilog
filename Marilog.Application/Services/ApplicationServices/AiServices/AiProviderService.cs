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

        public AiProviderService(IRepository<AiProvider> repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<AiProviderResponse>> GetAllAsync(CancellationToken ct)
        {
            var items = await _repo.Query()
                .AsNoTracking()
                .Select(ToResponse)
                .ToListAsync(ct);

            return items;
        }

        public async Task<AiProviderResponse?> GetByIdAsync(int id, CancellationToken ct)
        {
             return await _repo.Query()
                .AsNoTracking()
                .Where(ai => ai.Id == id)
                .Select(ToResponse)
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task<AiProviderResponse> CreateAsync(CreateAiProviderRequest request, CancellationToken ct)
        {
            var entity = AiProvider.Create
            (
                request.Name,
                request.ProviderType,
                request.BaseUrl,
                request.ModelIdentifier,
                request.MaxInputTokens,
                request.MaxOutputTokens,
                request.ChunkOverlapTokens,
                request.ApiKeyName,
                request.ApiKeyEncrypted,
                request.ApiVersion,
                request.DefaultTemperature,
                request.SupportsStreaming
            );

            await _repo.AddAsync(entity, ct);
            await _repo.SaveChangesAsync(ct);

            return new AiProviderResponse
            {
                SupportsStreaming = entity.SupportsStreaming,
                ApiKeyName = request.ApiKeyEncrypted,
                ApiVersion = request.ApiVersion,
                BaseUrl = request.BaseUrl,
                DefaultTemperature = request.DefaultTemperature,
                ChunkOverlapTokens = request.ChunkOverlapTokens,
                Id = entity.Id,
                MaxInputTokens = entity.MaxInputTokens,
                MaxOutputTokens = entity.MaxOutputTokens,
                ModelIdentifier = request.ModelIdentifier,
                Name = request.Name,
                ProviderType = request.ProviderType
            };
        }

        public async Task<bool> UpdateAsync(int id, UpdateAiProviderRequest request, CancellationToken ct)
        {
            var entity = await _repo.GetByIdAsync(id, ct);
            if (entity is null) return false;
            entity.Update(request.Name, request.BaseUrl,
                          request.ModelIdentifier, request.MaxInputTokens,
                          request.MaxOutputTokens, request.ChunkOverlapTokens,
                          request.ApiKeyName, request.ApiKeyEncrypted, true);
 
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

        public async Task DeactiveAsync(int id, CancellationToken ct)
        {
            var entity = await _repo.GetByIdAsync(id, ct);
            if (entity == null)
                throw new ArgumentNullException(nameof(entity) + "not found");

            if (entity.IsActive == false)
                return;

            entity.Deactivate();
            _repo.Update(entity);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task ActivateAsync (int id, CancellationToken ct)
        {
            var entity = await _repo.GetByIdAsync(id, ct);
            if (entity == null)
                throw new ArgumentNullException(nameof(entity) + "not found");

            if (entity.IsActive == true)
                return;

            entity.Activate();
            _repo.Update(entity);
            await _repo.SaveChangesAsync(ct);
        }

        //----Privates---------------------------------------------------------

        private static readonly Expression<Func<AiProvider, AiProviderResponse>> ToResponse = e=> new AiProviderResponse
        
            {
                Id = e.Id,
                Name = e.Name,
                ProviderType = e.ProviderType,
                BaseUrl = e.BaseUrl,
                ModelIdentifier = e.ModelIdentifier,
                ApiVersion = e.ApiVersion,
                MaxInputTokens = e.MaxInputTokens,
                MaxOutputTokens = e.MaxOutputTokens,
                ChunkOverlapTokens = e.ChunkOverlapTokens,
                DefaultTemperature = e.DefaultTemperature,
                ApiKeyName = e.ApiKeyName,
                SupportsStreaming = e.SupportsStreaming
            };
    }
}