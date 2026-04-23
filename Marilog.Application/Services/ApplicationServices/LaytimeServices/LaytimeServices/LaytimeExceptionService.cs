using Marilog.Application.Interfaces.Services.Laytime;
using Marilog.Contracts.DTOs.Requests.LayTimeDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.CharterLaytimeServices;
using Marilog.Domain.Entities.LaytimeEntities;
using Marilog.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Marilog.Application.Services.ApplicationServices.LaytimeServices.LaytimeServices
{
    public class LaytimeExceptionService : ILaytimeExceptionService
    {
        private readonly IRepository<LaytimeException> _exceptionRepo;
        private readonly ILaytimeHelpper _helpper;
        public LaytimeExceptionService(IRepository<LaytimeException> repository, ILaytimeHelpper laytimeHelpper)
        {
            _exceptionRepo = repository;
            _helpper = laytimeHelpper;

        }
        public async Task<LaytimeExceptionResponse> AddExceptionAsync(
           int calculationId,
           AddLaytimeExceptionRequest request,
           CancellationToken cancellationToken = default)
        {
            var calculation = await _helpper.GetCalculationOrThrowAsync(
                calculationId, withIncludes: true, cancellationToken);

            var exception = LaytimeException.Create(
                calculationId,
                request.From,
                request.To,
                request.ExceptionType,
                request.Factor,
                request.Notes,
                request.LinkedSofEventId);

            var result = calculation.AddException(exception);
            if (result.IsFailure)
                throw new InvalidOperationException(result.Error);

            await _exceptionRepo.SaveChangesAsync(cancellationToken);

            return _helpper.MapExceptionResponse(exception);
        }

        public async Task UpdateExceptionAsync(
            int exceptionId,
            UpdateLaytimeExceptionRequest request,
            CancellationToken cancellationToken = default)
        {
            var exception = await _exceptionRepo
                .Query()
                .FirstOrDefaultAsync(x => x.Id == exceptionId, cancellationToken)
                ?? throw new KeyNotFoundException($"Exception {exceptionId} not found.");

            exception.UpdateFactor(request.Factor);
            exception.UpdateNotes(request.Notes);

            await _exceptionRepo.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveExceptionAsync(
            int calculationId,
            int exceptionId,
            CancellationToken cancellationToken = default)
        {
            var calculation = await _helpper.GetCalculationOrThrowAsync(
                calculationId, withIncludes: true, cancellationToken);

            var result = calculation.RemoveException(exceptionId);
            if (result.IsFailure)
                throw new KeyNotFoundException(result.Error);

            await _exceptionRepo.SaveChangesAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<LaytimeExceptionResponse>> GetExceptionsAsync(
            int calculationId,
            CancellationToken cancellationToken = default)
        {
            var exceptions = await _exceptionRepo
                .Query()
                .Where(x => x.LaytimeCalculationId == calculationId)
                .OrderBy(x => x.From)
                .ToListAsync(cancellationToken);

            return exceptions.Select(_helpper.MapExceptionResponse).ToList();
        }
    }
}
