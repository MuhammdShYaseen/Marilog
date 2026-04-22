using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Contracts.Interfaces.Services.CharterLaytimeServices
{
    public interface ITimeSheetExportService
    {
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
    }
}
