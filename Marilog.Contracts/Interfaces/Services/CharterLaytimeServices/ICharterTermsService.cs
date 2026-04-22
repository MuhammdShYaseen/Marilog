using Marilog.Contracts.DTOs.Requests.LayTimeDTOs;
using Marilog.Contracts.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Contracts.Interfaces.Services.CharterLaytimeServices
{
    public interface ICharterTermsService
    {
        /// <summary>تهيئة Charter Terms لعقد جديد</summary>
        Task<CharterTermsResponse> InitializeCharterTermsAsync(InitializeCharterTermsRequest request, CancellationToken cancellationToken = default);

        /// <summary>تحديث شروط عملية التحميل</summary>
        Task UpdateLoadingTermsAsync(int contractId, CargoOperationTermsRequest request, CancellationToken cancellationToken = default);

        /// <summary>تحديث شروط عملية التفريغ</summary>
        Task UpdateDischargingTermsAsync(int contractId, CargoOperationTermsRequest request, CancellationToken cancellationToken = default);

        /// <summary>تحديث شروط Demurrage</summary>
        Task UpdateDemurrageTermsAsync(int contractId, DemurrageTermsRequest request, CancellationToken cancellationToken = default);

        /// <summary>تحديث شروط Despatch</summary>
        Task UpdateDespatchTermsAsync(int contractId, DespatchTermsRequest request, CancellationToken cancellationToken = default);

        /// <summary>تحديث خيارات Laytime Rule Options</summary>
        Task UpdateRuleOptionsAsync(int contractId, LaytimeRuleOptionsRequest request, CancellationToken cancellationToken = default);

        /// <summary>جلب Charter Terms لعقد معين</summary>
        Task<CharterTermsResponse> GetCharterTermsAsync(int contractId, CancellationToken cancellationToken = default);
    }
}
