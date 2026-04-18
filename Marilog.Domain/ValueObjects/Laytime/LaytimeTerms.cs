namespace Marilog.Domain.ValueObjects.Laytime
{
    /// <summary>
    /// Container رئيسي لكل شروط الـ Laytime المستخرجة من Charter Party.
    /// Value Object غير قابل للتعديل — كل تحديث ينتج instance جديد (With* pattern).
    /// يُخزَّن كـ Owned Entity داخل CharterTerms.
    /// </summary>
    public class LaytimeTerms
    {
        /// <summary>شروط عملية التحميل — مطلوبة في Charter Parties التحميل</summary>
        public CargoOperationTerms? Loading { get; private set; }

        /// <summary>شروط عملية التفريغ — مطلوبة في Charter Parties التفريغ</summary>
        public CargoOperationTerms? Discharging { get; private set; }

        /// <summary>شروط الـ Demurrage — مطلوبة دائماً</summary>
        public DemurrageTerms Demurrage { get; private set; } = default!;

        /// <summary>شروط الـ Despatch — اختيارية (بعض العقود لا تتضمنها)</summary>
        public DespatchTerms? Despatch { get; private set; }

        /// <summary>الخيارات التشغيلية المستخرجة من Charter Party</summary>
        public LaytimeRuleOptions RuleOptions { get; private set; } = default!;

        private LaytimeTerms() { } // للـ EF Core

        // ─────────────────────────────
        // Factory Methods
        // ─────────────────────────────

        /// <summary>إنشاء LaytimeTerms مكتملة — للاستخدام عند توفر كل بنود العقد</summary>
        public static LaytimeTerms Create(
            DemurrageTerms demurrage,
            LaytimeRuleOptions ruleOptions,
            CargoOperationTerms? loading = null,
            CargoOperationTerms? discharging = null,
            DespatchTerms? despatch = null)
        {
            ArgumentNullException.ThrowIfNull(demurrage);
            ArgumentNullException.ThrowIfNull(ruleOptions);

            if (loading is null && discharging is null)
                throw new ArgumentException(
                    "At least one of Loading or Discharging terms must be provided.");

            return new LaytimeTerms
            {
                Loading = loading,
                Discharging = discharging,
                Demurrage = demurrage,
                Despatch = despatch,
                RuleOptions = ruleOptions
            };
        }

        /// <summary>
        /// إنشاء LaytimeTerms فارغة — للاستخدام عند تسجيل العقد قبل اكتمال التفاصيل.
        /// يجب تحديثها قبل إجراء أي حساب Laytime.
        /// </summary>
        public static LaytimeTerms CreateEmpty() =>
            new()
            {
                Loading = null,
                Discharging = null,
                Demurrage = DemurrageTerms.CreateDefault(),
                Despatch = null,
                RuleOptions = LaytimeRuleOptions.CreateDefault()
            };

        // ─────────────────────────────
        // Validation Helper
        // ─────────────────────────────

        /// <summary>هل هذه الـ Terms جاهزة لإجراء حساب Laytime؟</summary>
        public bool IsReadyForCalculation() =>
            (Loading is not null || Discharging is not null) &&
            Demurrage.RateUsdPerDay > 0;

        // ─────────────────────────────
        // With* Pattern (Immutable Updates)
        // ─────────────────────────────

        public LaytimeTerms WithLoading(CargoOperationTerms loading)
        {
            ArgumentNullException.ThrowIfNull(loading);
            return Clone(loading: loading);
        }

        public LaytimeTerms WithDischarging(CargoOperationTerms discharging)
        {
            ArgumentNullException.ThrowIfNull(discharging);
            return Clone(discharging: discharging);
        }

        public LaytimeTerms WithDemurrage(DemurrageTerms demurrage)
        {
            ArgumentNullException.ThrowIfNull(demurrage);
            return Clone(demurrage: demurrage);
        }

        public LaytimeTerms WithDespatch(DespatchTerms despatch)
        {
            ArgumentNullException.ThrowIfNull(despatch);
            return Clone(despatch: despatch);
        }

        public LaytimeTerms WithRuleOptions(LaytimeRuleOptions ruleOptions)
        {
            ArgumentNullException.ThrowIfNull(ruleOptions);
            return Clone(ruleOptions: ruleOptions);
        }

        // ─── Equality ───

        public override bool Equals(object? obj) =>
            obj is LaytimeTerms other &&
            Equals(Loading, other.Loading) &&
            Equals(Discharging, other.Discharging) &&
            Equals(Demurrage, other.Demurrage) &&
            Equals(Despatch, other.Despatch) &&
            Equals(RuleOptions, other.RuleOptions);

        public override int GetHashCode() =>
            HashCode.Combine(Loading, Discharging, Demurrage, Despatch, RuleOptions);

        // ─────────────────────────────
        // Private
        // ─────────────────────────────

        private LaytimeTerms Clone(
            CargoOperationTerms? loading = null,
            CargoOperationTerms? discharging = null,
            DemurrageTerms? demurrage = null,
            DespatchTerms? despatch = null,
            LaytimeRuleOptions? ruleOptions = null) =>
            new()
            {
                Loading = loading ?? Loading,
                Discharging = discharging ?? Discharging,
                Demurrage = demurrage ?? Demurrage,
                Despatch = despatch ?? Despatch,
                RuleOptions = ruleOptions ?? RuleOptions
            };
    }
}