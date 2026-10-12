using Marilog.Contracts.DTOs.Requests.LayTimeDTOs;
using Marilog.Contracts.DTOs.Responses;


namespace Marilog.Contracts.Interfaces.Services.CharterLaytimeServices
{
    public interface ILaytimeExceptionService
    {
        /// <summary>تسجيل استثناء (تأخير أو انقطاع) على Calculation</summary>
        Task<LaytimeExceptionResponse> AddExceptionAsync(int calculationId, AddLaytimeExceptionRequest request, CancellationToken cancellationToken = default);

        /// <summary>تعديل ملاحظات أو Factor لاستثناء موجود</summary>
        Task UpdateExceptionAsync(int calculationId, int exceptionId, UpdateLaytimeExceptionRequest request, CancellationToken cancellationToken = default);

        /// <summary>حذف استثناء</summary>
        Task RemoveExceptionAsync(int calculationId, int exceptionId, CancellationToken cancellationToken = default);

        /// <summary>جلب جميع الاستثناءات لـ Calculation</summary>
        Task<IReadOnlyList<LaytimeExceptionResponse>> GetExceptionsAsync(int calculationId, CancellationToken cancellationToken = default);
    }
}
