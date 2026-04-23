using Marilog.Application.Interfaces.Services.Laytime;
using Marilog.Contracts.DTOs.Requests.LayTimeDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.CharterLaytimeServices;
using Marilog.Domain.Entities.LaytimeEntities;
using Marilog.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.Services.ApplicationServices.LaytimeServices.LaytimeServices
{
    public class SofEventService : ISofEventService
    {
        private readonly ILaytimeHelpper _helpper;
        private readonly IRepository<SofEvent> _sofRepo;
        public SofEventService(IRepository<SofEvent> repository, ILaytimeHelpper laytimeHelpper)
        {
            _helpper = laytimeHelpper;
            _sofRepo = repository;
        }
        public async Task<SofEventResponse> AddSofEventAsync(
            int calculationId,
            AddSofEventRequest request,
            CancellationToken cancellationToken = default)
        {
            var calculation = await _helpper.GetCalculationOrThrowAsync(
                calculationId, withIncludes: false, cancellationToken);

            var sofEvent = SofEvent.Create(
                calculationId,
                request.EventTime,
                request.EventType,
                request.ImpactType,
                request.Factor,
                request.Description);

            var addResult = calculation.AddSofEvent(sofEvent);
            if (addResult.IsFailure)
                throw new InvalidOperationException(addResult.Error);

            await _sofRepo.SaveChangesAsync(cancellationToken);

            return _helpper.MapSofEventResponse(sofEvent);
        }

        public async Task<IReadOnlyList<SofEventResponse>> AddSofEventsBatchAsync(
            int calculationId,
            IEnumerable<AddSofEventRequest> requests,
            CancellationToken cancellationToken = default)
        {
            var calculation = await _helpper.GetCalculationOrThrowAsync(
                calculationId, withIncludes: true, cancellationToken);

            var responses = new List<SofEventResponse>();

            foreach (var request in requests.OrderBy(r => r.EventTime))
            {
                var sofEvent = SofEvent.Create(
                    calculationId,
                    request.EventTime,
                    request.EventType,
                    request.ImpactType,
                    request.Factor,
                    request.Description);

                var addResult = calculation.AddSofEvent(sofEvent);
                if (addResult.IsFailure)
                    throw new InvalidOperationException(
                        $"Failed to add event at {request.EventTime:yyyy-MM-dd HH:mm}: {addResult.Error}");

                responses.Add(_helpper.MapSofEventResponse(sofEvent));
            }

            await _sofRepo.SaveChangesAsync(cancellationToken);

            return responses;
        }

        public async Task UpdateSofEventImpactAsync(
            int sofEventId,
            UpdateSofEventImpactRequest request,
            CancellationToken cancellationToken = default)
        {
            var sofEvent = await _sofRepo
                .Query()
                .FirstOrDefaultAsync(x => x.Id == sofEventId, cancellationToken)
                ?? throw new KeyNotFoundException($"SOF event {sofEventId} not found.");

            sofEvent.UpdateImpact(request.ImpactType, request.Factor);
            await _sofRepo.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveSofEventAsync(
            int calculationId,
            int sofEventId,
            CancellationToken cancellationToken = default)
        {
            var calculation = await _helpper.GetCalculationOrThrowAsync(
                calculationId, withIncludes: true, cancellationToken);

            var result = calculation.RemoveSofEvent(sofEventId);
            if (result.IsFailure)
                throw new KeyNotFoundException(result.Error);

            await _sofRepo.SaveChangesAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<SofEventResponse>> GetSofEventsAsync(
            int calculationId,
            CancellationToken cancellationToken = default)
        {
            var events = await _sofRepo
                .Query()
                .Where(x => x.LaytimeCalculationId == calculationId)
                .OrderBy(x => x.EventTime)
                .ToListAsync(cancellationToken);

            return events.Select(_helpper.MapSofEventResponse).ToList();
        }
    }
}
