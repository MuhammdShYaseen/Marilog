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
                                LaytimeSegmentBuild build,
                                decimal demurrageRateUsdPerDay,
                                decimal despatchRateUsdPerDay,
                                DespatchBasis despatchBasis)
        {
            var usedTicks = build.Segments.Sum(s => s.CountedDuration.Ticks);

            // LT-14: decimal math from ticks; no double round-trip
            var usedDays = (decimal)usedTicks / TimeSpan.TicksPerDay;
            var allowedDays = build.AllowedDays; // LT-16: quantity/rate or fixed hours

            decimal demurrageAmount = 0;
            decimal despatchAmount = 0;

            if (usedDays > allowedDays)
            {
                // LT-05: segments after LaytimeExpiredAt already carry demurrage factors,
                // so used − allowed is exactly the counted time after expiry
                demurrageAmount = Math.Round(
                    (usedDays - allowedDays) * demurrageRateUsdPerDay, 2, MidpointRounding.AwayFromZero);
            }
            else
            {
                // LT-07: all time saved = calendar time from completion to the hypothetical expiry
                var savedDays = despatchBasis == DespatchBasis.OnAllTimeSaved && build.AllTimeSaved is { } allTime
                    ? (decimal)allTime.Ticks / TimeSpan.TicksPerDay
                    : allowedDays - usedDays;

                despatchAmount = Math.Round(
                    savedDays * despatchRateUsdPerDay, 2, MidpointRounding.AwayFromZero);
            }

            return LaytimeResult.Create(
                allowedDays,
                usedDays,
                demurrageAmount,
                despatchAmount);
        }

        public LaytimeSegmentBuild BuildSegments(
                                                    LaytimeCalculation calculation,
                                                    CargoOperationTerms operationTerms,
                                                    LaytimeRuleOptions ruleOptions,
                                                    bool onceOnDemurrage,
                                                    decimal? allowedDaysOverride = null)
        {
            var events = calculation.SofEvents
                        .OrderBy(e => e.EventTime)
                        .ThenBy(e => e.Id) // LT-01: deterministic order for same-time events
                        .ToList();
            // LT-16: allowed time comes from quantity/rate or a fixed number of hours
            // LT-08: under reversible laytime the service passes the remaining pooled time
            var allowedDays = allowedDaysOverride ?? operationTerms.AllowedDays(calculation.CargoQuantityMt);

            if (events.Count < 2)
                return new LaytimeSegmentBuild(allowedDays, [], null, null);

            // ─────────────────────────────
            // 1) بناء Segments خام من الفارق بين كل حدثين متتاليين
            // ─────────────────────────────
            var segments = new List<LaytimeSegment>(events.Count - 1);

            for (int i = 0; i < events.Count - 1; i++)
            {
                var current = events[i];
                var next = events[i + 1];

                // LT-01: skip zero-length pairs; the last event of a same-time group governs the next period
                if (next.EventTime == current.EventTime)
                    continue;

                segments.Add(LaytimeSegment.Create(
                    calculationId: calculation.Id,
                    from: current.EventTime,
                    to: next.EventTime,
                    impactType: current.ImpactType,
                    factor: current.Factor,
                    reason: current.EventType.ToString()));
            }
            // All events share the same timestamp — nothing to count
            if (segments.Count == 0)
                return new LaytimeSegmentBuild(allowedDays, segments, null, null);

            // 1a) LT-12: explicit laytime markers entered by the user bound the counting window
            var markerWindows = new List<(DateTime From, DateTime To, string Reason)>();

            var commencedMarker = events.FirstOrDefault(e => e.EventType == SofEventType.LaytimeCommenced);
            if (commencedMarker is not null && commencedMarker.EventTime > events[0].EventTime)
                markerWindows.Add((events[0].EventTime, commencedMarker.EventTime, "Before Laytime Commenced"));

            var completedMarker = events.FirstOrDefault(e => e.EventType == SofEventType.LaytimeCompleted);
            if (completedMarker is not null && completedMarker.EventTime < events[^1].EventTime)
                markerWindows.Add((completedMarker.EventTime, events[^1].EventTime, "After Laytime Completed"));

            if (markerWindows.Count > 0)
            {
                var bounded = new List<LaytimeSegment>(segments.Count);
                foreach (var segment in segments)
                {
                    if (segment.ImpactType == LaytimeImpactType.NoCount)
                        bounded.Add(segment);
                    else
                        bounded.AddRange(ApplyExclusions(calculation.Id, segment, markerWindows));
                }
                segments = bounded;
            }

            // 1b) LT-09: draft survey / shifting windows not counted unless the contract says they count
            var surveyAndShifting = new List<(DateTime From, DateTime To, string Reason)>();

            if (!ruleOptions.DraftSurveyCounts)
                surveyAndShifting.AddRange(FindWindows(
                    events, SofEventType.DraftSurveyStart, SofEventType.DraftSurveyEnd, "Draft Survey - Not Counted"));

            if (!ruleOptions.AllowShiftingTime)
                surveyAndShifting.AddRange(FindWindows(
                    events, SofEventType.ShiftingStart, SofEventType.ShiftingEnd, "Shifting - Not Counted"));

            if (surveyAndShifting.Count > 0)
            {
                var windowed = new List<LaytimeSegment>(segments.Count);
                foreach (var segment in segments)
                {
                    if (segment.ImpactType == LaytimeImpactType.NoCount)
                        windowed.Add(segment);
                    else
                        windowed.AddRange(ApplyExclusions(calculation.Id, segment, surveyAndShifting));
                }
                segments = windowed;
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

            // LT-06: holidays are entered by the user as Holiday exceptions and
            // excluded under the same calendar rules as the weekend
            var holidayPeriods = GetHolidayPeriods(calculation.Exceptions, operationTerms.CalendarType, ruleOptions);
            var calendarPeriods = GetExcludedPeriods(
                    overallFrom, overallTo, operationTerms.CalendarType, ruleOptions)
                         .Concat(holidayPeriods)
                         .ToList();

            // LT-11: "unless used" — time the user records as worked inside a weekend/holiday
            // is taken out of the exclusion and counted at the factor the user entered
            var unlessUsed = calculation.Exceptions
                .Where(e => e.ExceptionType == LaytimeExceptionType.UnlessUsed)
                .ToList();

            var excludedPeriods = SubtractUsed(calendarPeriods, unlessUsed);
            var usedWindows = UsedWindows(calculation.Id, calendarPeriods, unlessUsed);

            if (excludedPeriods.Count > 0)
            {
                var excludedSegments = new List<LaytimeSegment>(segments.Count);
                foreach (var segment in segments)
                {
                    // لا داعي لتقسيم Segment أصلاً NoCount — النتيجة صفر بأي الأحوال
                    if (segment.ImpactType == LaytimeImpactType.NoCount)
                    {
                        excludedSegments.Add(segment);
                        continue;
                    }

                    excludedSegments.AddRange(
                        ApplyExclusions(calculation.Id, segment, excludedPeriods));
                }
                segments = excludedSegments;
            }

            // 4) LT-04: LaytimeException records
            segments = ApplyExceptions(calculation.Id, segments, calculation.Exceptions
                        .Where(e => e.ExceptionType is not (LaytimeExceptionType.Holiday or LaytimeExceptionType.UnlessUsed))
                        .Concat(usedWindows)
                        .ToList());

            // 5) LT-05: find the expiry instant; after it, demurrage rules apply
            var allowedTicks = (long)(allowedDays * TimeSpan.TicksPerDay);
            var completedAt = FindCompletion(events, calculation.OperationType);

            var (finalSegments, expiredAt) = ApplyExpiry(
                calculation.Id, segments, allowedTicks, completedAt,
                onceOnDemurrage, events, calculation.Exceptions);

            // 6) LT-07: if laytime never expired, run the clock on past completion
            //    under calendar rules to find the hypothetical expiry
            TimeSpan? allTimeSaved = null;
            if (expiredAt is null)
            {
                var remainingTicks = allowedTicks - finalSegments.Sum(s => s.CountedDuration.Ticks);
                if (remainingTicks > 0)
                {
                    var hypotheticalExpiry = FindHypotheticalExpiry(completedAt, remainingTicks, operationTerms.CalendarType, ruleOptions, holidayPeriods);
                    allTimeSaved = hypotheticalExpiry - completedAt;
                }
            }

            return new LaytimeSegmentBuild(allowedDays, finalSegments, expiredAt, allTimeSaved);
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

            // LT-15: the valid NOR is the first one tendered with no rejection at or after it
            var norTendered = events.FirstOrDefault(e =>
                e.EventType == SofEventType.NorTendered &&
                !events.Any(r => r.EventType == SofEventType.NorRejected && r.EventTime >= e.EventTime));
            if (norTendered is null)
                return segments;

            var earliestCountable = norTendered.EventTime.AddHours(noticeHours);

            // LT-10: "unless sooner commenced" — notice ends early if cargo work starts before it runs out
            if (operationTerms.UnlessSoonerCommenced)
            {
                var workStarted = operationTerms.OperationType == OperationType.Loading
                    ? SofEventType.LoadingCommenced
                    : SofEventType.DischargingCommenced;

                var commenced = events.FirstOrDefault(e =>
                    e.EventType == workStarted && e.EventTime >= norTendered.EventTime);

                if (commenced is not null && commenced.EventTime < earliestCountable)
                    earliestCountable = commenced.EventTime;
            }

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
        /// <summary>
        /// يحسب فترات عطلة نهاية الأسبوع المستبعدة من احتساب الـ Laytime.
        /// SHINC / SSHINC → لا استبعاد. SHEX / WeatherWorkingDay → الـ weekend مستبعد
        /// ما لم يكن WeekendIncluded = true.
        /// LT-11: الـ weekend نافذة زمنية — تبدأ يوم WeekendDay1 الساعة WeekendStartTime
        /// وتنتهي في اليوم التالي لآخر يوم عطلة الساعة WeekendEndTime
        /// (القيمة الافتراضية 00:00 = أيام كاملة كما في السابق).
        /// العطل الرسمية تُدخَل كـ Holiday exceptions (انظر GetHolidayPeriods).
        /// </summary>
        internal static IReadOnlyList<(DateTime From, DateTime To, string Reason)> GetExcludedPeriods(
            DateTime from,
            DateTime to,
            LaytimeCalendarType calendarType,
            LaytimeRuleOptions ruleOptions)
        {
            var excluded = new List<(DateTime, DateTime, string)>();

            var appliesCalendarExclusion =
                calendarType is LaytimeCalendarType.SHEX or LaytimeCalendarType.WeatherWorkingDay;

            if (!appliesCalendarExclusion || ruleOptions.WeekendIncluded || ruleOptions.WeekendDay1 is not { } firstDay)
                return excluded;

            var lastDay = ruleOptions.WeekendDay2 ?? firstDay;

            // Two consecutive days entered in reverse order (e.g. Saturday, Friday)
            if (((int)firstDay - (int)lastDay + 7) % 7 == 1)
                (firstDay, lastDay) = (lastDay, firstDay);

            var spanDays = ((int)lastDay - (int)firstDay + 7) % 7 + 1;
            var startTime = ruleOptions.WeekendStartTime ?? TimeSpan.Zero;
            var endTime = ruleOptions.WeekendEndTime ?? TimeSpan.Zero;

            // Start a week early so a window that began before 'from' is still caught
            for (var day = from.Date.AddDays(-7); day <= to.Date; day = day.AddDays(1))
            {
                if (day.DayOfWeek != firstDay)
                    continue;

                var windowStart = day + startTime;
                var windowEnd = day.AddDays(spanDays) + endTime;

                var clippedStart = windowStart > from ? windowStart : from;
                var clippedEnd = windowEnd < to ? windowEnd : to;

                if (clippedEnd > clippedStart)
                    excluded.Add((clippedStart, clippedEnd, "Weekend - Excluded"));
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
        // LT-04: Laytime Exceptions
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Splits segments at exception boundaries; each piece takes
        /// min(segment factor, lowest overlapping exception factor) to avoid double reduction.
        /// </summary>
        private static List<LaytimeSegment> ApplyExceptions(
            int calculationId,
            List<LaytimeSegment> segments,
            IReadOnlyCollection<LaytimeException> exceptions)
        {
            if (exceptions.Count == 0)
                return segments;

            var result = new List<LaytimeSegment>(segments.Count);

            foreach (var segment in segments)
            {
                var overlapping = exceptions
                    .Where(e => e.Overlaps(segment.From, segment.To))
                    .ToList();

                if (overlapping.Count == 0)
                {
                    result.Add(segment);
                    continue;
                }

                var cuts = overlapping
                    .SelectMany(e => new[] { e.From, e.To })
                    .Where(t => t > segment.From && t < segment.To)
                    .Append(segment.From)
                    .Append(segment.To)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToList();

                for (int i = 0; i < cuts.Count - 1; i++)
                {
                    var from = cuts[i];
                    var to = cuts[i + 1];

                    var exception = overlapping
                        .Where(e => e.Overlaps(from, to))
                        .MinBy(e => e.Factor);

                    var segmentFactor = EffectiveFactor(segment);

                    if (exception is null || exception.Factor >= segmentFactor)
                    {
                        result.Add(LaytimeSegment.Create(
                            calculationId, from, to, segment.ImpactType, segment.Factor, segment.Reason));
                        continue;
                    }

                    result.Add(CreatePiece(
                        calculationId, from, to, exception.Factor, $"Exception - {exception.ExceptionType}"));
                }
            }

            return result;
        }

        // ─────────────────────────────────────────────────────────────────
        // LT-05: Once on demurrage, always on demurrage
        // ─────────────────────────────────────────────────────────────────

        // Vessel-side stoppages that still reduce time after laytime expiry.
        // The proportion is the data-entry factor (e.g. 1 of 4 cranes down → 0.75).
        private static readonly HashSet<SofEventType> OwnerFaultEvents =
            [SofEventType.CraneBreakdownStart];

        private static readonly HashSet<LaytimeExceptionType> OwnerFaultExceptions =
            [LaytimeExceptionType.CraneBreakdown, LaytimeExceptionType.MechanicalFailure];

        /// <summary>
        /// Walks segments in time order until counted time reaches allowed time,
        /// splits the segment that crosses that instant, and (when onceOnDemurrage)
        /// re-rates everything between expiry and completion under demurrage rules.
        /// When onceOnDemurrage is false the laytime rules keep applying after expiry.
        /// </summary>
        private static (List<LaytimeSegment> Segments, DateTime? ExpiredAt) ApplyExpiry(
            int calculationId,
            List<LaytimeSegment> segments,
            long allowedTicks,
            DateTime completedAt,
            bool onceOnDemurrage,
            List<SofEvent> events,
            IReadOnlyCollection<LaytimeException> exceptions)
        {
            var result = new List<LaytimeSegment>(segments.Count + 1);
            long accumulated = 0;
            DateTime? expiredAt = null;

            LaytimeSegment AfterExpiry(LaytimeSegment s) =>
                onceOnDemurrage && s.From < completedAt
                    ? ToDemurrage(calculationId, s, events, exceptions)
                    : s;

            foreach (var segment in segments)
            {
                if (expiredAt is not null)
                {
                    result.Add(AfterExpiry(segment));
                    continue;
                }

                var counted = segment.CountedDuration.Ticks;
                if (counted == 0 || accumulated + counted < allowedTicks)
                {
                    accumulated += counted;
                    result.Add(segment);
                    continue;
                }

                // Laytime runs out inside (or at the end of) this segment
                var remaining = allowedTicks - accumulated;
                var expiry = segment.From.AddTicks((long)Math.Ceiling(remaining / EffectiveFactor(segment)));
                if (expiry > segment.To)
                    expiry = segment.To;

                expiredAt = expiry;
                // No laytime left at all (e.g. reversible pool already used up at an earlier port)
                if (expiry == segment.From)
                {
                    result.Add(AfterExpiry(segment));
                    continue;
                }
                if (expiry == segment.To)
                {
                    result.Add(segment);
                    continue;
                }

                result.Add(LaytimeSegment.Create(
                    calculationId, segment.From, expiry, segment.ImpactType, segment.Factor, segment.Reason));

                result.Add(AfterExpiry(LaytimeSegment.Create(
                    calculationId, expiry, segment.To, segment.ImpactType, segment.Factor, segment.Reason)));
            }

            return (result, expiredAt);
        }

        /// <summary>
        /// Demurrage rate for a piece after expiry: full time, except vessel-side
        /// stoppages, which keep their entered factor.
        /// </summary>
        private static LaytimeSegment ToDemurrage(
            int calculationId,
            LaytimeSegment segment,
            List<SofEvent> events,
            IReadOnlyCollection<LaytimeException> exceptions)
        {
            var factor = 1m;
            var reason = "On demurrage";

            // events are ordered by time then Id, so the last one at or before From governs this piece
            var governing = events.LastOrDefault(e => e.EventTime <= segment.From);
            if (governing is not null && OwnerFaultEvents.Contains(governing.EventType))
            {
                factor = EventFactor(governing);
                reason = $"On demurrage - {governing.EventType}";
            }

            var ownerException = exceptions
                .Where(e => OwnerFaultExceptions.Contains(e.ExceptionType) && e.Overlaps(segment.From, segment.To))
                .MinBy(e => e.Factor);

            if (ownerException is not null && ownerException.Factor < factor)
            {
                factor = ownerException.Factor;
                reason = $"On demurrage - {ownerException.ExceptionType}";
            }

            return CreatePiece(calculationId, segment.From, segment.To, factor, reason);
        }

        /// <summary>LaytimeCompleted if present, else the operation's completion event, else the last event.</summary>
        private static DateTime FindCompletion(List<SofEvent> events, OperationType operationType)
        {
            var operationCompleted = operationType == OperationType.Loading
                ? SofEventType.LoadingCompleted
                : SofEventType.DischargingCompleted;

            return events.FirstOrDefault(e => e.EventType == SofEventType.LaytimeCompleted)?.EventTime
                ?? events.FirstOrDefault(e => e.EventType == operationCompleted)?.EventTime
                ?? events[^1].EventTime;
        }

        // ─────────────────────────────────────────────────────────────────
        // LT-07: Despatch on all time saved
        // ─────────────────────────────────────────────────────────────────

        private static DateTime FindHypotheticalExpiry(
            DateTime start,
            long remainingTicks,
            LaytimeCalendarType calendarType,
            LaytimeRuleOptions ruleOptions,
            IReadOnlyList<(DateTime From, DateTime To, string Reason)> holidayPeriods)
        {
            // Weekends add at most 2 days per week; holidays add their own length.
            // This horizon always covers the remaining laytime.
            var holidayTicks = holidayPeriods.Sum(h => (h.To - h.From).Ticks);
            var horizon = start.AddTicks(remainingTicks * 2 + holidayTicks).AddDays(14);

            var excluded = GetExcludedPeriods(start, horizon, calendarType, ruleOptions)
                .Concat(holidayPeriods)
                .OrderBy(p => p.From);

            var cursor = start;
            foreach (var period in excluded)
            {
                // Weekend and holiday periods may overlap; skip what is already behind the cursor
                if (period.To <= cursor)
                    continue;

                var available = Math.Max(0, (period.From - cursor).Ticks);
                if (available >= remainingTicks)
                    break;

                remainingTicks -= available;
                cursor = period.To;
            }

            return cursor.AddTicks(remainingTicks);
        }

        // ─────────────────────────────────────────────────────────────────
        // Factor helpers
        // ─────────────────────────────────────────────────────────────────

        private static decimal EffectiveFactor(LaytimeSegment s) => s.ImpactType switch
        {
            LaytimeImpactType.FullCount => 1m,
            LaytimeImpactType.NoCount => 0m,
            _ => s.Factor
        };

        private static decimal EventFactor(SofEvent e) => e.ImpactType switch
        {
            LaytimeImpactType.FullCount => 1m,
            LaytimeImpactType.NoCount => 0m,
            _ => e.Factor
        };

        private static LaytimeSegment CreatePiece(
            int calculationId, DateTime from, DateTime to, decimal factor, string? reason) => factor switch
            {
                >= 1m => LaytimeSegment.Create(calculationId, from, to, LaytimeImpactType.FullCount, 1m, reason),
                <= 0m => LaytimeSegment.Create(calculationId, from, to, LaytimeImpactType.NoCount, 0m, reason),
                _ => LaytimeSegment.Create(calculationId, from, to, LaytimeImpactType.ProRata, factor, reason)
            };


        public static string FormatDuration(TimeSpan ts)
        {
            int days = (int)ts.TotalDays;
            int hours = ts.Hours;
            int minutes = ts.Minutes;

            return days > 0
                ? $"{days}d {hours:D2}h {minutes:D2}m"
                : $"{hours:D2}h {minutes:D2}m";
        }

        // ─────────────────────────────────────────────────────────────────
        // LT-09: Draft survey / shifting windows
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Pairs each start event with the first matching end event after it.
        /// A start with no end is left alone (its own SOF impact applies).
        /// </summary>
        private static List<(DateTime From, DateTime To, string Reason)> FindWindows(
            List<SofEvent> events,
            SofEventType startType,
            SofEventType endType,
            string reason)
        {
            var windows = new List<(DateTime From, DateTime To, string Reason)>();

            foreach (var start in events.Where(e => e.EventType == startType))
            {
                var end = events.FirstOrDefault(e =>
                    e.EventType == endType && e.EventTime > start.EventTime);

                if (end is not null)
                    windows.Add((start.EventTime, end.EventTime, reason));
            }

            return windows;
        }


        // ─────────────────────────────────────────────────────────────────
        // LT-06: Holidays
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Holiday exceptions entered by the user, excluded only when the calendar
        /// excludes holidays (SHEX / WWD) and the contract does not include them.
        /// </summary>
        private static List<(DateTime From, DateTime To, string Reason)> GetHolidayPeriods(
            IReadOnlyCollection<LaytimeException> exceptions,
            LaytimeCalendarType calendarType,
            LaytimeRuleOptions ruleOptions)
        {
            var calendarExcludesHolidays =
                calendarType is LaytimeCalendarType.SHEX or LaytimeCalendarType.WeatherWorkingDay;

            if (!calendarExcludesHolidays || ruleOptions.HolidaysIncluded)
                return [];

            return exceptions
                .Where(e => e.ExceptionType == LaytimeExceptionType.Holiday)
                .Select(e => (e.From, e.To, "Holiday - Excluded"))
                .ToList();
        }


        // ─────────────────────────────────────────────────────────────────
        // LT-11: Unless used
        // ─────────────────────────────────────────────────────────────────

        /// <summary>Excluded periods minus the time the user recorded as worked.</summary>
        private static List<(DateTime From, DateTime To, string Reason)> SubtractUsed(
            List<(DateTime From, DateTime To, string Reason)> periods,
            List<LaytimeException> used)
        {
            if (used.Count == 0)
                return periods;

            var result = new List<(DateTime From, DateTime To, string Reason)>();

            foreach (var period in periods)
            {
                var cursor = period.From;

                foreach (var u in used.Where(u => u.Overlaps(period.From, period.To)).OrderBy(u => u.From))
                {
                    if (u.From > cursor)
                        result.Add((cursor, u.From, period.Reason));

                    if (u.To > cursor)
                        cursor = u.To;
                }

                if (cursor < period.To)
                    result.Add((cursor, period.To, period.Reason));
            }

            return result;
        }

        /// <summary>
        /// The worked time that falls inside excluded periods, as transient exceptions
        /// carrying the user's factor (1 = unless used, 0.5 = half time used).
        /// These are never added to the calculation.
        /// </summary>
        private static List<LaytimeException> UsedWindows(
            int calculationId,
            List<(DateTime From, DateTime To, string Reason)> periods,
            List<LaytimeException> used)
        {
            var result = new List<LaytimeException>();

            foreach (var period in periods)
            {
                foreach (var u in used.Where(u => u.Overlaps(period.From, period.To)))
                {
                    var from = u.From > period.From ? u.From : period.From;
                    var to = u.To < period.To ? u.To : period.To;

                    result.Add(LaytimeException.Create(
                        calculationId, from, to, LaytimeExceptionType.UnlessUsed, u.Factor));
                }
            }

            return result;
        }
    }
}