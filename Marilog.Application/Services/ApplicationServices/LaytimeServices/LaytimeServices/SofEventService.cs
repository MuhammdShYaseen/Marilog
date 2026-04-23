using Marilog.Contracts.DTOs.Requests.LayTimeDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.CharterLaytimeServices;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.Services.ApplicationServices.LaytimeServices.LaytimeServices
{
    public class SofEventService : ISofEventService
    {
        public Task<SofEventResponse> AddSofEventAsync(int calculationId, AddSofEventRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<SofEventResponse>> AddSofEventsBatchAsync(int calculationId, IEnumerable<AddSofEventRequest> requests, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<SofEventResponse>> GetSofEventsAsync(int calculationId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task RemoveSofEventAsync(int calculationId, int sofEventId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task UpdateSofEventImpactAsync(int sofEventId, UpdateSofEventImpactRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
