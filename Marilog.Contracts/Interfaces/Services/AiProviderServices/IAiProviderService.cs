using Marilog.Contracts.DTOs.Requests.AiProviderDTOs;
using Marilog.Contracts.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Contracts.Interfaces.Services.AiProviderServices
{
    public interface IAiProviderService
    {
        Task<IEnumerable<AiProviderResponse>> GetAllAsync(CancellationToken ct);
        Task<AiProviderResponse?> GetByIdAsync(int id, CancellationToken ct);
        Task<AiProviderResponse> CreateAsync(CreateAiProviderRequest request, CancellationToken ct);
        Task<bool> UpdateAsync(int id, UpdateAiProviderRequest request, CancellationToken ct);
        Task<bool> DeleteAsync(int id, CancellationToken ct);
        Task DeactiveAsync(int id, CancellationToken ct);
        Task ActivateAsync(int id, CancellationToken ct);
    }
}
