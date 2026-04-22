using Marilog.Contracts.DTOs.Requests.LayTimeDTOs;
using Marilog.Contracts.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Contracts.Interfaces.Services.CharterLaytimeServices
{
    public interface ISofEventService
    {
        /// <summary>إضافة حدث SOF واحد</summary>
        Task<SofEventResponse> AddSofEventAsync(int calculationId, AddSofEventRequest request, CancellationToken cancellationToken = default);

        /// <summary>إضافة مجموعة أحداث SOF دفعة واحدة</summary>
        Task<IReadOnlyList<SofEventResponse>> AddSofEventsBatchAsync(int calculationId, IEnumerable<AddSofEventRequest> requests, CancellationToken cancellationToken = default);

        /// <summary>تعديل تأثير حدث SOF (ImpactType / Factor)</summary>
        Task UpdateSofEventImpactAsync(int sofEventId, UpdateSofEventImpactRequest request, CancellationToken cancellationToken = default);

        /// <summary>حذف حدث SOF</summary>
        Task RemoveSofEventAsync(int calculationId, int sofEventId, CancellationToken cancellationToken = default);

        /// <summary>جلب جميع SOF Events لـ Calculation مرتبة زمنياً</summary>
        Task<IReadOnlyList<SofEventResponse>> GetSofEventsAsync(int calculationId, CancellationToken cancellationToken = default);
    }
}
