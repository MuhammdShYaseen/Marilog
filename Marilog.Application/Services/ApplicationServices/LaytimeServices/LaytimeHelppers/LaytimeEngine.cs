using Marilog.Application.Interfaces.Services.Laytime;
using Marilog.Domain.Entities.LaytimeEntities;
using Marilog.Domain.ValueObjects.Laytime;
using Marilog.Kernel.Enums;

namespace Marilog.Application.Services.ApplicationServices.LaytimeServices.LaytimeHelppers
{
    public sealed class LaytimeEngine : ILaytimeEngine
    {
        public LaytimeResult Calculate(
            LaytimeCalculation calculation,
            IReadOnlyList<LaytimeSegment> segments,
            decimal loadingRateMtPerDay,
            decimal demurrageRateUsdPerDay,
            decimal despatchRateUsdPerDay)
        {
            var usedTicks = segments.Sum(s => s.CountedDuration.Ticks);

            var usedTime = TimeSpan.FromTicks(usedTicks);
            var usedDays = (decimal)usedTime.TotalDays;
            var allowedDays = calculation.CargoQuantityMt / loadingRateMtPerDay;
            var allowedTime = TimeSpan.FromDays((double)allowedDays);

            decimal demurrageAmount = 0;
            decimal despatchAmount = 0;

            if (usedTime > allowedTime)
            {
                var excessDays = (decimal)(usedTime - allowedTime).TotalDays;
                demurrageAmount = Math.Round(excessDays * demurrageRateUsdPerDay, 2);
            }
            else
            {
                var savedDays = (decimal)(allowedTime - usedTime).TotalDays;
                despatchAmount = Math.Round(savedDays * despatchRateUsdPerDay, 2);
            }

            return LaytimeResult.Create(
                allowedDays,
                usedDays,
                demurrageAmount,
                despatchAmount);
        }

        public IReadOnlyList<LaytimeSegment> BuildSegments(
            LaytimeCalculation calculation,
            CargoOperationTerms operationTerms,
            LaytimeRuleOptions ruleOptions)
        {
            var events = calculation.SofEvents
                .OrderBy(e => e.EventTime)
                .ToList();

            if (events.Count < 2)
                return [];

            // ─────────────────────────────
            // 1) بناء Segments خام من الفارق بين كل حدثين متتاليين
            // ─────────────────────────────
            var segments = new List<LaytimeSegment>(events.Count - 1);

            for (int i = 0; i < events.Count - 1; i++)
            {
                var current = events[i];
                var next = events[i + 1];

                segments.Add(LaytimeSegment.Create(
                    calculationId: calculation.Id,
                    from: current.EventTime,
                    to: next.EventTime,
                    impactType: current.ImpactType,
                    factor: current.Factor,
                    reason: current.EventType.ToString()));
            }

            // ─────────────────────────────
            // 2) فترة الإشعار (Notice Hours) — لا يُحتسب شيء قبل
            //    NorTendered + NoticeHours حتى لو كانت الأحداث تقول غير ذلك
            // ─────────────────────────────
            segments = ApplyNoticePeriod(calculation.Id, events, operationTerms, segments);

            // ─────────────────────────────
            // 3) استبعاد الأحد/العطل تلقائياً حسب CalendarType (SHEX/PWWD)
            //    ما لم يُلغَ ذلك صراحة عبر RuleOptions
            // ─────────────────────────────
            var overallFrom = events[0].EventTime;
            var overallTo = events[^1].EventTime;

            var excludedPeriods = GetExcludedPeriods(
                overallFrom, overallTo, operationTerms.CalendarType, ruleOptions);

            if (excludedPeriods.Count == 0)
                return segments;

            var finalSegments = new List<LaytimeSegment>(segments.Count);
            foreach (var segment in segments)
            {
                // لا داعي لتقسيم Segment أصلاً NoCount — النتيجة صفر بأي الأحوال
                if (segment.ImpactType == LaytimeImpactType.NoCount)
                {
                    finalSegments.Add(segment);
                    continue;
                }

                finalSegments.AddRange(
                    ApplyExclusions(calculation.Id, segment, excludedPeriods));
            }

            return finalSegments;
        }

        // ─────────────────────────────────────────────────────────────────
        // فترة الإشعار (Notice Period)
        // ─────────────────────────────────────────────────────────────────

        private static List<LaytimeSegment> ApplyNoticePeriod(
            int calculationId,
            List<SofEvent> events,
            CargoOperationTerms operationTerms,
            List<LaytimeSegment> segments)
        {
            var noticeHours = Convert.ToDouble(operationTerms.NoticeHours);
            if (noticeHours <= 0)
                return segments;

            var norTendered = events.FirstOrDefault(e => e.EventType == SofEventType.NorTendered);
            if (norTendered is null)
                return segments;

            var earliestCountable = norTendered.EventTime.AddHours(noticeHours);

            var result = new List<LaytimeSegment>(segments.Count);

            foreach (var segment in segments)
            {
                if (segment.To <= earliestCountable)
                {
                    // كامل الـ Segment داخل فترة الإشعار
                    result.Add(LaytimeSegment.Create(
                        calculationId, segment.From, segment.To,
                        LaytimeImpactType.NoCount, 0m, "Notice Period"));
                }
                else if (segment.From < earliestCountable)
                {
                    // الـ Segment يقطع حد فترة الإشعار — يُقسَّم لجزئين
                    result.Add(LaytimeSegment.Create(
                        calculationId, segment.From, earliestCountable,
                        LaytimeImpactType.NoCount, 0m, "Notice Period"));

                    result.Add(LaytimeSegment.Create(
                        calculationId, earliestCountable, segment.To,
                        segment.ImpactType, segment.Factor, segment.Reason));
                }
                else
                {
                    result.Add(segment);
                }
            }

            return result;
        }

        // ─────────────────────────────────────────────────────────────────
        // استبعاد الأحد / العطل (SHEX و PWWD)
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// يحسب الفترات المستبعدة تلقائياً من احتساب الـ Laytime
        /// بناءً على نوع التقويم (CalendarType) وخيارات العقد (RuleOptions).
        /// SHINC / SSHINC → لا استبعاد (كل الأيام تُحتسب).
        /// SHEX / WeatherWorkingDay → أيام عطلة نهاية الأسبوع (WeekendDay1/2 — قد تكون
        /// جمعة فقط، أو أحد فقط، أو جمعة+سبت، أو سبت+أحد... حسب تعريف العقد) مستبعدة
        /// ما لم يُلغَ ذلك عبر WeekendIncluded، والعطل الرسمية مستبعدة ما لم يُلغَ عبر
        /// HolidaysIncluded.
        /// ملاحظة: تحديد "هل هذا اليوم عطلة رسمية؟" يتطلب مصدر بيانات عطل
        /// (تقويم عطل حسب الميناء/الدولة) غير متوفر بعد في الكود الحالي —
        /// حالياً يُطبَّق استبعاد عطلة نهاية الأسبوع فقط، أما HolidaysIncluded=false
        /// بدون قائمة عطل فعلية فلن يستبعد أي يوم إضافي.
        /// </summary>
        internal static IReadOnlyList<(DateTime From, DateTime To, string Reason)> GetExcludedPeriods(
            DateTime from,
            DateTime to,
            LaytimeCalendarType calendarType,
            LaytimeRuleOptions ruleOptions,
            IReadOnlyCollection<DateOnly>? holidayDates = null)
        {
            var excluded = new List<(DateTime, DateTime, string)>();

            var appliesCalendarExclusion =
                calendarType is LaytimeCalendarType.SHEX or LaytimeCalendarType.WeatherWorkingDay;

            if (!appliesCalendarExclusion)
                return excluded;

            var excludeWeekend = !ruleOptions.WeekendIncluded;
            var excludeHolidays = !ruleOptions.HolidaysIncluded && holidayDates is { Count: > 0 };

            if (!excludeWeekend && !excludeHolidays)
                return excluded;

            var day = from.Date;
            var endDate = to.Date;

            while (day <= endDate)
            {
                var isWeekendDay = excludeWeekend && ruleOptions.IsWeekendDay(day.DayOfWeek);
                var isHoliday = excludeHolidays && holidayDates!.Contains(DateOnly.FromDateTime(day));

                if (isWeekendDay || isHoliday)
                {
                    var dayStart = day;
                    var dayEnd = day.AddDays(1);

                    var windowStart = dayStart > from ? dayStart : from;
                    var windowEnd = dayEnd < to ? dayEnd : to;

                    if (windowEnd > windowStart)
                    {
                        var reason = isWeekendDay && isHoliday
                            ? "Weekend & Holiday - Excluded"
                            : isWeekendDay
                                ? "Weekend - Excluded"
                                : "Holiday - Excluded";

                        excluded.Add((windowStart, windowEnd, reason));
                    }
                }

                day = day.AddDays(1);
            }

            return excluded;
        }

        /// <summary>
        /// يقسّم Segment واحد على أي فترات مستبعدة تتقاطع معه،
        /// فيتحول الجزء المتقاطع إلى NoCount ويبقى الباقي بتأثيره الأصلي.
        /// </summary>
        private static List<LaytimeSegment> ApplyExclusions(
            int calculationId,
            LaytimeSegment segment,
            IReadOnlyList<(DateTime From, DateTime To, string Reason)> excludedPeriods)
        {
            var overlapping = excludedPeriods
                .Where(p => p.From < segment.To && p.To > segment.From)
                .OrderBy(p => p.From)
                .ToList();

            if (overlapping.Count == 0)
                return [segment];

            var result = new List<LaytimeSegment>();
            var cursor = segment.From;

            foreach (var period in overlapping)
            {
                var excludedStart = period.From > cursor ? period.From : cursor;
                var excludedEnd = period.To < segment.To ? period.To : segment.To;

                if (excludedStart > cursor)
                {
                    result.Add(LaytimeSegment.Create(
                        calculationId, cursor, excludedStart,
                        segment.ImpactType, segment.Factor, segment.Reason));
                }

                if (excludedEnd > excludedStart)
                {
                    result.Add(LaytimeSegment.Create(
                        calculationId, excludedStart, excludedEnd,
                        LaytimeImpactType.NoCount, 0m, period.Reason));

                    cursor = excludedEnd;
                }
            }

            if (cursor < segment.To)
            {
                result.Add(LaytimeSegment.Create(
                    calculationId, cursor, segment.To,
                    segment.ImpactType, segment.Factor, segment.Reason));
            }

            return result;
        }

        // ─────────────────────────────────────────────────────────────────
        // Helper
        // ─────────────────────────────────────────────────────────────────

        public static string FormatDuration(TimeSpan ts)
        {
            int days = (int)ts.TotalDays;
            int hours = ts.Hours;
            int minutes = ts.Minutes;

            return days > 0
                ? $"{days}d {hours:D2}h {minutes:D2}m"
                : $"{hours:D2}h {minutes:D2}m";
        }
    }
}