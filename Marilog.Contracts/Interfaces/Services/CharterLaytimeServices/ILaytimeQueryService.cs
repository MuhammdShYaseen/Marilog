using Marilog.Contracts.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Contracts.Interfaces.Services.CharterLaytimeServices
{
    public interface ILaytimeQueryService
    {
        /// <summary>
        /// جلب Segments المحسوبة — تمثل الـ Time Sheet الفعلي
        /// (من SOF event إلى التالي مع ImpactType و CountedDuration)
        /// </summary>
        Task<IReadOnlyList<LaytimeSegmentResponse>> GetSegmentsAsync(int calculationId, CancellationToken cancellationToken = default);

    }
}
