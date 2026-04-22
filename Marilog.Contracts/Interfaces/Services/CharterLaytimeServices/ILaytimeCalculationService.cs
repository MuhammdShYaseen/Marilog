using Marilog.Contracts.DTOs.Requests.LayTimeDTOs;
using Marilog.Contracts.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Contracts.Interfaces.Services.CharterLaytimeServices
{
    public interface ILaytimeCalculationService
    {
        /// <summary>إنشاء Laytime Calculation جديد لرحلة في ميناء معين</summary>
        Task<LaytimeCalculationResponse> CreateCalculationAsync(CreateLaytimeCalculationRequest request, CancellationToken cancellationToken = default);

        /// <summary>جلب Calculation بالمعرف</summary>
        Task<LaytimeCalculationResponse> GetCalculationAsync(int calculationId, CancellationToken cancellationToken = default);

        /// <summary>جلب جميع Calculations لرحلة معينة</summary>
        Task<IReadOnlyList<LaytimeCalculationSummaryResponse>> GetCalculationsByVoyageAsync(int voyageId, CancellationToken cancellationToken = default);

        /// <summary>تشغيل حساب Laytime وبناء Segments من SOF Events</summary>
        Task<LaytimeResultResponse> ComputeAsync(int calculationId, CancellationToken cancellationToken = default);

        /// <summary>إعادة الحساب من البداية (Draft → Computed)</summary>
        Task<LaytimeResultResponse> RecomputeAsync(int calculationId, CancellationToken cancellationToken = default);

        /// <summary>تثبيت نتيجة الحساب نهائياً (Computed → Finalized)</summary>
        Task FinalizeAsync(int calculationId, CancellationToken cancellationToken = default);
    }
}
