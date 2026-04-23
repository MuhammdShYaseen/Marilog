using Marilog.Contracts.DTOs.Requests.LayTimeDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.CharterLaytimeServices;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.Services.ApplicationServices.LaytimeServices
{
    public class LaytimeExceptionService : ILaytimeExceptionService
    {
        public Task<LaytimeExceptionResponse> AddExceptionAsync(int calculationId, AddLaytimeExceptionRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<LaytimeExceptionResponse>> GetExceptionsAsync(int calculationId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task RemoveExceptionAsync(int calculationId, int exceptionId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task UpdateExceptionAsync(int exceptionId, UpdateLaytimeExceptionRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
