namespace Marilog.Domain.ValueObjects.Laytime
{
    /// <summary>
    /// شروط الـ Demurrage المستخرجة من Charter Party.
    /// Value Object غير قابل للتعديل.
    /// </summary>
    public class DemurrageTerms
    {
        /// <summary>معدل الـ Demurrage بالدولار/يوم</summary>
        public decimal RateUsdPerDay { get; private set; }

        /// <summary>
        /// بند "Once on Demurrage, Always on Demurrage".
        /// إذا كان true: بمجرد دخول الـ Demurrage لا يوجد استثناء لأي تأخير لاحق.
        /// إذا كان false: يمكن استثناء التأخيرات التي لا تقع تحت مسؤولية المستأجر.
        /// </summary>
        public bool OnceOnDemurrageAlwaysOnDemurrage { get; private set; }

        private DemurrageTerms() { } // للـ EF Core

        public static DemurrageTerms Create(
            decimal rateUsdPerDay,
            bool onceOnDemurrageAlwaysOnDemurrage = false)
        {
            if (rateUsdPerDay <= 0)
                throw new ArgumentException(
                    "Demurrage rate must be greater than zero.", nameof(rateUsdPerDay));

            return new DemurrageTerms
            {
                RateUsdPerDay = rateUsdPerDay,
                OnceOnDemurrageAlwaysOnDemurrage = onceOnDemurrageAlwaysOnDemurrage
            };
        }

        /// <summary>قيم افتراضية — للاستخدام في CreateEmpty فقط</summary>
        internal static DemurrageTerms CreateDefault() =>
            new() { RateUsdPerDay = 0, OnceOnDemurrageAlwaysOnDemurrage = false };

        // ─── Equality ───

        public override bool Equals(object? obj) =>
            obj is DemurrageTerms other &&
            RateUsdPerDay == other.RateUsdPerDay &&
            OnceOnDemurrageAlwaysOnDemurrage == other.OnceOnDemurrageAlwaysOnDemurrage;

        public override int GetHashCode() =>
            HashCode.Combine(RateUsdPerDay, OnceOnDemurrageAlwaysOnDemurrage);
    }
}