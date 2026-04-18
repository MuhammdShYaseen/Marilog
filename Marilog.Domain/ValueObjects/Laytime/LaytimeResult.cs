namespace Marilog.Domain.ValueObjects.Laytime
{
    /// <summary>
    /// نتيجة حساب الـ Laytime النهائية — Value Object غير قابل للتعديل.
    /// يُنتجه LaytimeCalculatorEngine ويُخزَّن داخل LaytimeCalculation.
    /// </summary>
    public class LaytimeResult
    {
        /// <summary>الوقت المسموح به بالأيام بناءً على الكمية والمعدل</summary>
        public decimal AllowedDays { get; private set; }

        /// <summary>الوقت الفعلي المحتسب بالأيام</summary>
        public decimal UsedDays { get; private set; }

        /// <summary>
        /// الرصيد = Allowed - Used.
        /// موجب = Despatch (تبكير)، سالب = Demurrage (تأخير).
        /// </summary>
        public decimal BalanceDays { get; private set; }

        /// <summary>مبلغ الـ Demurrage بالدولار (0 إذا لا يوجد تأخير)</summary>
        public decimal DemurrageAmount { get; private set; }

        /// <summary>مبلغ الـ Despatch بالدولار (0 إذا لا يوجد تبكير)</summary>
        public decimal DespatchAmount { get; private set; }

        /// <summary>true = الرحلة انتهت بتأخير، false = انتهت بتبكير أو في الوقت</summary>
        public bool IsDemurrage { get; private set; }

        private LaytimeResult() { } // للـ EF Core

        public static LaytimeResult Create(
            decimal allowedDays,
            decimal usedDays,
            decimal demurrageAmount,
            decimal despatchAmount)
        {
            if (allowedDays <= 0)
                throw new ArgumentException(
                    "Allowed days must be greater than zero.", nameof(allowedDays));

            if (usedDays < 0)
                throw new ArgumentException(
                    "Used days cannot be negative.", nameof(usedDays));

            if (demurrageAmount < 0)
                throw new ArgumentException(
                    "Demurrage amount cannot be negative.", nameof(demurrageAmount));

            if (despatchAmount < 0)
                throw new ArgumentException(
                    "Despatch amount cannot be negative.", nameof(despatchAmount));

            var balance = allowedDays - usedDays;

            return new LaytimeResult
            {
                AllowedDays = Math.Round(allowedDays, 4),
                UsedDays = Math.Round(usedDays, 4),
                BalanceDays = Math.Round(balance, 4),
                DemurrageAmount = Math.Round(demurrageAmount, 2),
                DespatchAmount = Math.Round(despatchAmount, 2),
                IsDemurrage = balance < 0
            };
        }

        // ─── Equality ───

        public override bool Equals(object? obj) =>
            obj is LaytimeResult other &&
            AllowedDays == other.AllowedDays &&
            UsedDays == other.UsedDays &&
            DemurrageAmount == other.DemurrageAmount &&
            DespatchAmount == other.DespatchAmount;

        public override int GetHashCode() =>
            HashCode.Combine(AllowedDays, UsedDays, DemurrageAmount, DespatchAmount);
    }
}