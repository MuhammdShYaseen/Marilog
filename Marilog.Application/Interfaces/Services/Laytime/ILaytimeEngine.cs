using Marilog.Domain.Entities.LaytimeEntities;
using Marilog.Domain.ValueObjects.Laytime;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.Interfaces.Services.Laytime
{
    public interface ILaytimeEngine
    {
        /// <summary>
        /// يبني LaytimeSegments من SOF Events المرتبة زمنياً
        /// ثم يحسب النتيجة النهائية ويعيدها.
        /// </summary>
        LaytimeResult Calculate(
            LaytimeCalculation calculation,
            decimal loadingRateMtPerDay,
            decimal demurrageRateUsdPerDay,
            decimal despatchRateUsdPerDay);

        /// <summary>يبني الـ Segments فقط بدون حساب النتيجة</summary>
        IReadOnlyList<LaytimeSegment> BuildSegments(LaytimeCalculation calculation);
    }
}
