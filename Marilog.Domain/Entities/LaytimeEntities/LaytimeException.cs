using Marilog.Domain.Common;
using Marilog.Kernel.Enums;

namespace Marilog.Domain.Entities.LaytimeEntities
{
    /// <summary>
    /// يمثل استثناءً زمنياً محدداً يؤثر على حساب الـ Laytime.
    /// أمثلة: أمطار، عطل رافعة، تفتيش، إضراب.
    /// قد يُستخرج تلقائياً من SOF أو يُضاف يدوياً كـ override.
    /// </summary>
    public class LaytimeException : Entity
    {
        public int LaytimeCalculationId { get; private set; }

        public DateTime From { get; private set; }

        public DateTime To { get; private set; }

        public LaytimeExceptionType ExceptionType { get; private set; }

        /// <summary>
        /// نسبة تأثير الاستثناء على الاحتساب.
        /// 0 = لا يحتسب كلياً، 0.5 = نصف الوقت، 1 = يحتسب كاملاً.
        /// </summary>
        public decimal Factor { get; private set; }

        public string? Notes { get; private set; }

        /// <summary>ربط اختياري بحدث SOF مصدر هذا الاستثناء</summary>
        public int? LinkedSofEventId { get; private set; }

        // ─── Computed ───
        public TimeSpan Duration => To - From;

        private LaytimeException() { }

        public static LaytimeException Create(
            int calculationId,
            DateTime from,
            DateTime to,
            LaytimeExceptionType type,
            decimal factor,
            string? notes = null,
            int? linkedSofEventId = null)
        {
            if (to <= from)
                throw new ArgumentException(
                    "Exception end must be after start.", nameof(to));

            if (factor < 0m || factor > 1m)
                throw new ArgumentException(
                    "Factor must be between 0 and 1.", nameof(factor));

            return new LaytimeException
            {
                LaytimeCalculationId = calculationId,
                From = from,
                To = to,
                ExceptionType = type,
                Factor = factor,
                Notes = notes?.Trim(),
                LinkedSofEventId = linkedSofEventId
            };
        }

        /// <summary>هل يتداخل هذا الاستثناء مع فترة زمنية معينة؟</summary>
        public bool Overlaps(DateTime from, DateTime to) =>
            From < to && from < To;

        public void UpdateNotes(string? notes)
        {
            Notes = notes?.Trim() ?? string.Empty;
        }

        public void UpdateFactor(decimal factor)
        {
            if (factor < 0m || factor > 1m)
                throw new ArgumentException(
                    "Factor must be between 0 and 1.", nameof(factor));

            Factor = factor;
        }
    }
}