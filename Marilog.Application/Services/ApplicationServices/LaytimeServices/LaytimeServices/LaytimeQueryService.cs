using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.CharterLaytimeServices;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.Services.ApplicationServices.LaytimeServices.LaytimeServices
{
    public class LaytimeQueryService : ILaytimeQueryService
    {
        public Task<IReadOnlyList<LaytimeSegmentResponse>> GetSegmentsAsync(int calculationId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
