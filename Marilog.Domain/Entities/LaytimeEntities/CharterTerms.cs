using Marilog.Domain.Common;
using Marilog.Domain.ValueObjects.Laytime;

namespace Marilog.Domain.Entities.LaytimeEntities
{
    /// <summary>
    /// يحتوي على الشروط التجارية والتشغيلية لـ Charter Party المرتبطة بالعقد.
    /// علاقة 1:1 مع AContract.
    /// </summary>
    public class CharterTerms : Entity
    {
        public int ContractId { get; private set; }

        public decimal CargoQuantityMt { get; private set; }

        /// <summary>
        /// شروط الـ Laytime الكاملة — مطلوبة دائماً.
        /// تُخزَّن كـ Owned Entity في نفس جدول CharterTerms.
        /// </summary>
        public LaytimeTerms LaytimeTerms { get; private set; } = default!;

        private CharterTerms() { }

        /// <summary>إنشاء CharterTerms مكتملة مع كل الشروط</summary>
        public static CharterTerms Create(
            int contractId,
            decimal cargoQuantityMt,
            LaytimeTerms laytimeTerms)
        {
            ArgumentNullException.ThrowIfNull(laytimeTerms);

            if (cargoQuantityMt <= 0)
                throw new ArgumentException(
                    "Cargo quantity must be greater than zero.", nameof(cargoQuantityMt));

            return new CharterTerms
            {
                ContractId = contractId,
                CargoQuantityMt = cargoQuantityMt,
                LaytimeTerms = laytimeTerms
            };
        }

        /// <summary>
        /// إنشاء CharterTerms بيانات أولية فارغة عند تسجيل العقد قبل اكتمال التفاصيل.
        /// يجب تحديثها لاحقاً قبل إجراء أي حساب Laytime.
        /// </summary>
        public static CharterTerms CreateDraft(int contractId)
        {
            return new CharterTerms
            {
                ContractId = contractId,
                CargoQuantityMt = 0,
                LaytimeTerms = LaytimeTerms.CreateEmpty()
            };
        }

        // ─────────────────────────────
        // Updates
        // ─────────────────────────────

        public void UpdateCargoQuantity(decimal quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException(
                    "Cargo quantity must be greater than zero.", nameof(quantity));

            CargoQuantityMt = quantity;
            Touch();
        }

        public void UpdateLaytimeTerms(LaytimeTerms laytimeTerms)
        {
            ArgumentNullException.ThrowIfNull(laytimeTerms);
            LaytimeTerms = laytimeTerms;
            Touch();
        }
    }
}