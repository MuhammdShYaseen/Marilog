using Marilog.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;
using Marilog.Kernel.Enums;

namespace Marilog.Domain.Entities.LaytimeEntities
{
    /// <summary>
    /// يمثل فترة زمنية محسوبة بين حدثين متتاليين من SOF.
    /// تُبنى بواسطة LaytimeSegmentBuilder ولا تُنشأ يدوياً.
    /// تُحفظ في DB للـ audit trail القانوني.
    /// </summary>
    public class LaytimeSegment : Entity
    {
        public int LaytimeCalculationId { get; private set; }

        public DateTime From { get; private set; }

        public DateTime To { get; private set; }

        public LaytimeImpactType ImpactType { get; private set; }

        /// <summary>نسبة الاحتساب — فعّالة عند ProRata فقط (0 → 1)</summary>
        public decimal Factor { get; private set; }

        public string? Reason { get; private set; }

        // ─── Computed — لا تُخزَّن في DB ───

        [NotMapped]
        public TimeSpan Duration => To - From;

        [NotMapped]
        public TimeSpan CountedDuration => ImpactType switch
        {
            LaytimeImpactType.FullCount => Duration,
            LaytimeImpactType.NoCount => TimeSpan.Zero,
            LaytimeImpactType.ProRata => TimeSpan.FromTicks(
                                               (long)(Duration.Ticks * (double)Factor)),
            _ => Duration
        };

        private LaytimeSegment() { }

        public static LaytimeSegment Create(
            int calculationId,
            DateTime from,
            DateTime to,
            LaytimeImpactType impactType,
            decimal factor,
            string? reason = null)
        {
            if (to <= from)
                throw new ArgumentException(
                    "Segment end must be after start.", nameof(to));

            // Factor = 0 مقبول (NoCount عبر ProRata)، السالب غير مقبول
            if (factor < 0m || factor > 1m)
                throw new ArgumentException(
                    "Factor must be between 0 and 1.", nameof(factor));

            return new LaytimeSegment
            {
                LaytimeCalculationId = calculationId,
                From = from,
                To = to,
                ImpactType = impactType,
                Factor = factor,
                Reason = reason?.Trim()
            };
        }

        public void UpdateImpact(LaytimeImpactType impactType, decimal factor)
        {
            if (factor < 0m || factor > 1m)
                throw new ArgumentException(
                    "Factor must be between 0 and 1.", nameof(factor));

            ImpactType = impactType;
            Factor = factor;
        }

        public void UpdateReason(string? reason)
        {
            Reason = reason?.Trim() ?? string.Empty;
        }
    }
}