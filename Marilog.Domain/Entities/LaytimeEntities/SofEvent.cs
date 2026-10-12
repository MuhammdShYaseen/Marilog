using Marilog.Domain.Common;
using Marilog.Kernel.Enums;

namespace Marilog.Domain.Entities.LaytimeEntities
{
    /// <summary>
    /// يمثل حدثاً واحداً من Statement of Facts.
    /// كل سطر في SOF = نقطة زمنية واحدة (EventTime).
    /// الفترات الزمنية بين الأحداث تُحسب لاحقاً في LaytimeSegment.
    /// </summary>
    public class SofEvent : Entity
    {
        public int LaytimeCalculationId { get; private set; }

        /// <summary>وقت وقوع الحدث — نقطة زمنية واحدة</summary>
        public DateTime EventTime { get; private set; }

        public SofEventType EventType { get; private set; }

        /// <summary>
        /// تأثير هذا الحدث على الـ Laytime من لحظته حتى الحدث التالي.
        /// يستخدمه LaytimeSegmentBuilder لبناء الـ segments.
        /// </summary>
        public LaytimeImpactType ImpactType { get; private set; }

        /// <summary>نسبة الاحتساب — فعّالة عند ImpactType = ProRata فقط (0 → 1)</summary>
        public decimal Factor { get; private set; }

        public string? Description { get; private set; }

        private SofEvent() { }

        public static SofEvent Create(
            int calculationId,
            DateTime eventTime,
            SofEventType eventType,
            LaytimeImpactType impactType,
            decimal factor = 1.0m,
            string? description = null)
        {
            if (eventTime == default)
                throw new ArgumentException("Event time is required.", nameof(eventTime));

            if (!Enum.IsDefined(eventType))
                throw new ArgumentException(
                    $"Undefined SOF event type: {(int)eventType}.", nameof(eventType));

            if (!Enum.IsDefined(impactType))
                throw new ArgumentException(
                    $"Undefined impact type: {(int)impactType}.", nameof(impactType));

            if (factor is < 0m or > 1m)
                throw new ArgumentException(
                    "Factor must be between 0 and 1.", nameof(factor));

            return new SofEvent
            {
                LaytimeCalculationId = calculationId,
                EventTime = eventTime,
                EventType = eventType,
                ImpactType = impactType,
                Factor = factor,
                Description = description?.Trim()
            };
        }

        internal void UpdateImpact(LaytimeImpactType impactType, decimal factor)
        {
            if (!Enum.IsDefined(impactType))
                throw new ArgumentException(
                    $"Undefined impact type: {(int)impactType}.", nameof(impactType));

            if (factor is < 0m or > 1m)
                throw new ArgumentException(
                    "Factor must be between 0 and 1.", nameof(factor));

            ImpactType = impactType;
            Factor = factor;
        }

        internal void UpdateDescription(string? description)
        {
            Description = description?.Trim() ?? string.Empty;
        }
    }
}