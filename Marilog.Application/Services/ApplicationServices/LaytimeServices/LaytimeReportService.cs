using Marilog.Contracts.DTOs.Requests.LayTimeDTOs;
using Marilog.Contracts.Interfaces.Services.CharterLaytimeServices;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.Services.ApplicationServices.LaytimeServices
{
    public class LaytimeReportService : ILaytimeReportService
    {
        public Task<byte[]> GenerateContractLaytimeReportAsync(int contractId, ReportFormat format, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> GenerateDelayReportAsync(int calculationId, ReportFormat format, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> GenerateDetailedReportAsync(int calculationId, ReportFormat format, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> GenerateSummaryReportAsync(int calculationId, ReportFormat format, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
