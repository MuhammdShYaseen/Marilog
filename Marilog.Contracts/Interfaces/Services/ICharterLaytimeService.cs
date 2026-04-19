using Marilog.Contracts.DTOs.Requests.LayTimeDTOs;
using Marilog.Contracts.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Contracts.Interfaces.Services
{
    /// <summary>
    /// واجهة خدمة Charter Party و Laytime الموحدة.
    /// تغطي دورة الحياة الكاملة: إنشاء الشروط → تسجيل SOF → الحساب → التقارير.
    /// </summary>
    public interface ICharterLaytimeService
    {
        // ═══════════════════════════════════════════════════════════════════
        // Charter Terms
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>تهيئة Charter Terms لعقد جديد</summary>
        Task<CharterTermsResponse> InitializeCharterTermsAsync(InitializeCharterTermsRequest request, CancellationToken cancellationToken = default);

        /// <summary>تحديث شروط عملية التحميل</summary>
        Task UpdateLoadingTermsAsync(int contractId, CargoOperationTermsRequest request, CancellationToken cancellationToken = default);

        /// <summary>تحديث شروط عملية التفريغ</summary>
        Task UpdateDischargingTermsAsync(int contractId, CargoOperationTermsRequest request, CancellationToken cancellationToken = default);

        /// <summary>تحديث شروط Demurrage</summary>
        Task UpdateDemurrageTermsAsync(int contractId, DemurrageTermsRequest request, CancellationToken cancellationToken = default);

        /// <summary>تحديث شروط Despatch</summary>
        Task UpdateDespatchTermsAsync( int contractId, DespatchTermsRequest request, CancellationToken cancellationToken = default);

        /// <summary>تحديث خيارات Laytime Rule Options</summary>
        Task UpdateRuleOptionsAsync(int contractId, LaytimeRuleOptionsRequest request, CancellationToken cancellationToken = default);

        /// <summary>جلب Charter Terms لعقد معين</summary>
        Task<CharterTermsResponse> GetCharterTermsAsync(int contractId, CancellationToken cancellationToken = default);

        // ═══════════════════════════════════════════════════════════════════
        // Laytime Calculation
        // ═══════════════════════════════════════════════════════════════════

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

        // ═══════════════════════════════════════════════════════════════════
        // SOF Events
        // ═══════════════════════════════════════════════════════════════════

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

        // ═══════════════════════════════════════════════════════════════════
        // Exceptions (Delays / Interruptions)
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>تسجيل استثناء (تأخير أو انقطاع) على Calculation</summary>
        Task<LaytimeExceptionResponse> AddExceptionAsync(int calculationId, AddLaytimeExceptionRequest request, CancellationToken cancellationToken = default);

        /// <summary>تعديل ملاحظات أو Factor لاستثناء موجود</summary>
        Task UpdateExceptionAsync(int exceptionId, UpdateLaytimeExceptionRequest request, CancellationToken cancellationToken = default);

        /// <summary>حذف استثناء</summary>
        Task RemoveExceptionAsync(int calculationId, int exceptionId, CancellationToken cancellationToken = default);

        /// <summary>جلب جميع الاستثناءات لـ Calculation</summary>
        Task<IReadOnlyList<LaytimeExceptionResponse>> GetExceptionsAsync(int calculationId, CancellationToken cancellationToken = default);

        // ═══════════════════════════════════════════════════════════════════
        // Segments
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// جلب Segments المحسوبة — تمثل الـ Time Sheet الفعلي
        /// (من SOF event إلى التالي مع ImpactType و CountedDuration)
        /// </summary>
        Task<IReadOnlyList<LaytimeSegmentResponse>> GetSegmentsAsync(int calculationId, CancellationToken cancellationToken = default);

        // ═══════════════════════════════════════════════════════════════════
        // Time Sheet Export (Excel)
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// توليد ملف Excel احترافي للـ Time Sheet يحتوي على:
        /// - معلومات الرحلة والعقد
        /// - جدول SOF Events مرتب زمنياً
        /// - الـ Segments مع ImpactType و Duration و CountedDuration
        /// - ملخص Allowed / Used / Balance
        /// - مبلغ Demurrage أو Despatch
        /// - ملاحظات الاستثناءات
        /// </summary>
        Task<byte[]> GenerateTimeSheetExcelAsync(int calculationId, CancellationToken cancellationToken = default);

        // ═══════════════════════════════════════════════════════════════════
        // Reports
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// تقرير ملخص — Summary Report (PDF أو Excel).
        /// يتضمن: Allowed vs Used، النتيجة النهائية، Demurrage/Despatch.
        /// </summary>
        Task<byte[]> GenerateSummaryReportAsync(int calculationId, ReportFormat format, CancellationToken cancellationToken = default);

        /// <summary>
        /// تقرير تفصيلي — Detailed Report (PDF أو Excel).
        /// يتضمن: كل SOF Events، كل Segments، كل Exceptions، الحسابات كاملة.
        /// </summary>
        Task<byte[]> GenerateDetailedReportAsync(int calculationId, ReportFormat format, CancellationToken cancellationToken = default);

        /// <summary>
        /// تقرير التأخيرات — Delay Report (PDF أو Excel).
        /// يتضمن: فترات التأخير فقط، أسبابها، مدتها، تأثيرها على الحساب.
        /// </summary>
        Task<byte[]> GenerateDelayReportAsync(int calculationId, ReportFormat format, CancellationToken cancellationToken = default);

        /// <summary>
        /// تقرير مقارنة Allowed vs Used لعقد كامل (جميع Calculations).
        /// مفيد لمتابعة أداء الأسطول عبر الرحلات.
        /// </summary>
        Task<byte[]> GenerateContractLaytimeReportAsync(int contractId, ReportFormat format, CancellationToken cancellationToken = default);
    }
}
