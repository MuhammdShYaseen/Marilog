using System;

namespace Marilog.Domain.ValueObjects.Laytime
{
    /// <summary>
    /// خيارات تشغيلية مستخرجة من Charter Party تتحكم بكيفية احتساب الـ Laytime.
    /// </summary>
    public class LaytimeRuleOptions
    {
        public bool DraftSurveyCounts { get; private set; }

        public bool HolidaysIncluded { get; private set; }

        /// <summary>
        /// هل أيام نهاية الأسبوع (WeekendDay1/2) محتسبة ضمن الـ Laytime أم مستبعدة.
        /// true = محتسبة (لا يوجد استبعاد)، false = مستبعدة.
        /// </summary>
        public bool WeekendIncluded { get; private set; }

        /// <summary>اليوم الأول لعطلة نهاية الأسبوع (مثال: Friday). null = لا يوجد عطلة أسبوعية محددة.</summary>
        public DayOfWeek? WeekendDay1 { get; private set; }

        /// <summary>اليوم الثاني لعطلة نهاية الأسبوع (اختياري — مثال: Saturday عندما تكون العطلة يومين).</summary>
        public DayOfWeek? WeekendDay2 { get; private set; }

        public bool TimeReversible { get; private set; }

        public bool AllowShiftingTime { get; private set; }

        private LaytimeRuleOptions()
        {

        }

        public static LaytimeRuleOptions Create(
            bool draftSurveyCounts,
            bool holidaysIncluded,
            bool weekendIncluded,
            DayOfWeek? weekendDay1,
            DayOfWeek? weekendDay2,
            bool timeReversible,
            bool allowShiftingTime)
        {
            if (!weekendIncluded && weekendDay1 is null)
                throw new ArgumentException(
                    "WeekendDay1 must be specified when WeekendIncluded is false (a weekend day must be defined to exclude it).",
                    nameof(weekendDay1));

            if (weekendDay1 is not null && weekendDay1 == weekendDay2)
                throw new ArgumentException(
                    "WeekendDay1 and WeekendDay2 cannot be the same day.", nameof(weekendDay2));

            if (weekendDay1 is null && weekendDay2 is not null)
                throw new ArgumentException(
                    "WeekendDay2 cannot be set without WeekendDay1.", nameof(weekendDay2));

            return new LaytimeRuleOptions
            {
                DraftSurveyCounts = draftSurveyCounts,
                HolidaysIncluded = holidaysIncluded,
                WeekendIncluded = weekendIncluded,
                WeekendDay1 = weekendDay1,
                WeekendDay2 = weekendDay2,
                TimeReversible = timeReversible,
                AllowShiftingTime = allowShiftingTime
            };
        }

        /// <summary>افتراضي محايد: لا يوجد استبعاد لعطلة أسبوعية أو عطل (يطابق سلوك النظام قبل هذا التعديل)</summary>
        public static LaytimeRuleOptions CreateDefault() =>
            new()
            {
                DraftSurveyCounts = false,
                HolidaysIncluded = false,
                WeekendIncluded = true,
                WeekendDay1 = DayOfWeek.Friday,
                WeekendDay2 = null,
                TimeReversible = false,
                AllowShiftingTime = false
            };

        /// <summary>هل هذا اليوم يقع ضمن عطلة نهاية الأسبوع المُعرَّفة بهذا العقد؟</summary>
        public bool IsWeekendDay(DayOfWeek day) =>
            day == WeekendDay1 || day == WeekendDay2;
    }
}