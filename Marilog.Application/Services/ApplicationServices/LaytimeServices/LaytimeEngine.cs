using Marilog.Application.Interfaces.Services;
using Marilog.Domain.Entities.LaytimeEntities;
using Marilog.Domain.ValueObjects.Laytime;

namespace Marilog.Application.Services.ApplicationServices.LaytimeServices
{
    public sealed class LaytimeEngine : ILaytimeEngine
    {
        public LaytimeResult Calculate(LaytimeCalculation calculation, decimal loadingRateMtPerDay, decimal demurrageRateUsdPerDay, decimal despatchRateUsdPerDay)
        {
            var segments = BuildSegments(calculation);

            // جمع الوقت المحتسب فعلياً
            var usedTicks = segments
                .Sum(s => s.CountedDuration.Ticks);

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

        public IReadOnlyList<LaytimeSegment> BuildSegments(LaytimeCalculation calculation)
        {
            var events = calculation.SofEvents
                .OrderBy(e => e.EventTime)
                .ToList();

            if (events.Count < 2)
                return [];

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

            return segments;
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
