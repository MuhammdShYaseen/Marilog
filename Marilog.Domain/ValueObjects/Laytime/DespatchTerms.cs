using Marilog.Domain.Entities.LaytimeEntities;

namespace Marilog.Domain.ValueObjects.Laytime
{
    /// <summary>
    /// شروط الـ Despatch المستخرجة من Charter Party.
    /// Value Object غير قابل للتعديل.
    /// ملاحظة: بعض العقود لا تتضمن Despatch — في هذه الحالة لا تُنشأ هذه الـ VO أصلاً
    /// ويبقى LaytimeTerms.Despatch = null.
    /// </summary>
    public class DespatchTerms
    {
        /// <summary>معدل الـ Despatch بالدولار/يوم</summary>
        public decimal RateUsdPerDay { get; private set; }

        /// <summary>
        /// أساس احتساب وقت التبكير:
        /// OnWorkingTimeSaved = الوقت الفعلي المُوفَّر في ساعات العمل فقط.
        /// OnAllTimeSaved     = كل الوقت المُوفَّر بما في ذلك العطل.
        /// </summary>
        public DespatchBasis Basis { get; private set; }

        private DespatchTerms() { } // للـ EF Core

        public static DespatchTerms Create(
            decimal rateUsdPerDay,
            DespatchBasis basis)
        {
            if (rateUsdPerDay <= 0)
                throw new ArgumentException(
                    "Despatch rate must be greater than zero.", nameof(rateUsdPerDay));

            return new DespatchTerms
            {
                RateUsdPerDay = rateUsdPerDay,
                Basis = basis
            };
        }

        // ─── Equality ───

        public override bool Equals(object? obj) =>
            obj is DespatchTerms other &&
            RateUsdPerDay == other.RateUsdPerDay &&
            Basis == other.Basis;

        public override int GetHashCode() =>
            HashCode.Combine(RateUsdPerDay, Basis);
    }
}