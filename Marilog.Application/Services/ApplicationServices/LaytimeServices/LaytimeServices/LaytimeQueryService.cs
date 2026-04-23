using Marilog.Application.Interfaces.Services;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.CharterLaytimeServices;
using Marilog.Domain.Entities.LaytimeEntities;
using Marilog.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Marilog.Application.Services.ApplicationServices.LaytimeServices.LaytimeServices
{
    public class LaytimeQueryService : ILaytimeQueryService
    {
        private readonly IRepository<LaytimeSegment> _segmentRepo;
        private readonly ILaytimeHelpper _helpper;
        public LaytimeQueryService(IRepository<LaytimeSegment> repository, ILaytimeHelpper laytimeHelpper)
        {
            _segmentRepo = repository;
            _helpper = laytimeHelpper;
        }
        public async Task<IReadOnlyList<LaytimeSegmentResponse>> GetSegmentsAsync(
            int calculationId,
            CancellationToken cancellationToken = default)
        {
            var segments = await _segmentRepo
                .Query()
                .Where(x => x.LaytimeCalculationId == calculationId)
                .OrderBy(x => x.From)
                .ToListAsync(cancellationToken);

            return segments.Select(_helpper.MapSegmentResponse).ToList();
        }
    }
}
