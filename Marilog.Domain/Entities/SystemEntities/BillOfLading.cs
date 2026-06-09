using Marilog.Domain.Common;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Kernel.Enums;

namespace Marilog.Domain.Entities.SystemEntities
{
    public class BillOfLading : Entity
    {
        // ── Core Identity ───────────────────────────────────────────────────
        public int VoyageID { get; private set; }
        public Voyage Voyage { get; private set; } = null!;

        public string BlNumber { get; private set; } = null!;
        public BlType BlType { get; private set; }
        public BlIssuanceType IssuanceType { get; private set; }

        // ── الأطراف الثلاثة ─────────────────────────────────────────────────
        public int ShipperCompanyID { get; private set; }
        public Company ShipperCompany { get; private set; } = null!;

        public int? ConsigneeCompanyID { get; private set; }
        public Company? ConsigneeCompany { get; private set; }
        public string? ConsigneeToOrder { get; private set; }

        public int? NotifyPartyCompanyID { get; private set; }
        public Company? NotifyPartyCompany { get; private set; }

        public int CarrierCompanyID { get; private set; }
        public Company CarrierCompany { get; private set; } = null!;

        // ── الموانئ ─────────────────────────────────────────────────────────
        public int PortOfLoadingID { get; private set; }
        public Port PortOfLoading { get; private set; } = null!;

        public int PortOfDischargeID { get; private set; }
        public Port PortOfDischarge { get; private set; } = null!;

        public int? PlaceOfReceiptPortID { get; private set; }
        public Port? PlaceOfReceipt { get; private set; }

        public int? PlaceOfDeliveryPortID { get; private set; }
        public Port? PlaceOfDelivery { get; private set; }

        // ── البضاعة ─────────────────────────────────────────────────────────
        public string CargoDescription { get; private set; } = null!;
        public string? HsCode { get; private set; }
        public decimal GrossWeightMT { get; private set; }
        public decimal? VolumeM3 { get; private set; }
        public int? PackageCount { get; private set; }
        public string? PackageType { get; private set; }
        public string? MarksAndNumbers { get; private set; }

        // ── الشروط المالية ──────────────────────────────────────────────────
        public FreightTerms FreightTerms { get; private set; }
        public string? FreightAmount { get; private set; }
        public string? Incoterms { get; private set; }

        // ── التواريخ ────────────────────────────────────────────────────────
        public DateOnly IssueDate { get; private set; }
        public string? PlaceOfIssue { get; private set; }
        public DateOnly? OnBoardDate { get; private set; }

        // ── النسخ الأصلية ───────────────────────────────────────────────────
        public int OriginalCopiesCount { get; private set; } = 3;

        // ── MBL/HBL ─────────────────────────────────────────────────────────
        public int? MasterBlID { get; private set; }
        public BillOfLading? MasterBl { get; private set; }

        public string? Notes { get; private set; }

        private BillOfLading() { }

        // ── Create ───────────────────────────────────────────────────────────
        public static BillOfLading Create(
            int voyageId,
            string blNumber,
            BlType blType,
            BlIssuanceType issuanceType,
            int shipperCompanyId,
            int carrierCompanyId,
            int portOfLoadingId,
            int portOfDischargeId,
            string cargoDescription,
            decimal grossWeightMt,
            FreightTerms freightTerms,
            DateOnly issueDate,
            int? consigneeCompanyId = null,
            string? consigneeToOrder = null,
            int? notifyPartyCompanyId = null,
            string? hsCode = null,
            decimal? volumeM3 = null,
            int? packageCount = null,
            string? packageType = null,
            string? marksAndNumbers = null,
            string? freightAmount = null,
            string? incoterms = null,
            string? placeOfIssue = null,
            DateOnly? onBoardDate = null,
            int originalCopiesCount = 3,
            int? masterBlId = null,
            int? placeOfReceiptPortId = null,
            int? placeOfDeliveryPortId = null,
            string? notes = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(blNumber);
            ArgumentException.ThrowIfNullOrWhiteSpace(cargoDescription);
            if (grossWeightMt <= 0) throw new ArgumentException("GrossWeightMT must be positive.");
            if (shipperCompanyId <= 0) throw new ArgumentException("Invalid ShipperCompanyID.");
            if (carrierCompanyId <= 0) throw new ArgumentException("Invalid CarrierCompanyID.");

            if (blType == BlType.Straight && consigneeCompanyId == null)
                throw new InvalidOperationException("Straight BL requires a named Consignee.");

            if (blType == BlType.OrderBl && consigneeCompanyId == null && string.IsNullOrWhiteSpace(consigneeToOrder))
                throw new InvalidOperationException("Order BL requires either a Consignee or a 'To Order' instruction.");

            if (issuanceType == BlIssuanceType.House && masterBlId == null)
                throw new InvalidOperationException("House BL must reference a Master BL.");

            return new BillOfLading
            {
                VoyageID = voyageId,
                BlNumber = blNumber,
                BlType = blType,
                IssuanceType = issuanceType,
                ShipperCompanyID = shipperCompanyId,
                CarrierCompanyID = carrierCompanyId,
                ConsigneeCompanyID = consigneeCompanyId,
                ConsigneeToOrder = consigneeToOrder,
                NotifyPartyCompanyID = notifyPartyCompanyId,
                PortOfLoadingID = portOfLoadingId,
                PortOfDischargeID = portOfDischargeId,
                PlaceOfReceiptPortID = placeOfReceiptPortId,
                PlaceOfDeliveryPortID = placeOfDeliveryPortId,
                CargoDescription = cargoDescription,
                HsCode = hsCode,
                GrossWeightMT = grossWeightMt,
                VolumeM3 = volumeM3,
                PackageCount = packageCount,
                PackageType = packageType,
                MarksAndNumbers = marksAndNumbers,
                FreightTerms = freightTerms,
                FreightAmount = freightAmount,
                Incoterms = incoterms,
                IssueDate = issueDate,
                PlaceOfIssue = placeOfIssue,
                OnBoardDate = onBoardDate,
                OriginalCopiesCount = originalCopiesCount,
                MasterBlID = masterBlId,
                Notes = notes
            };
        }

        // ── Update العادية (كل شيء غير الحساس) ─────────────────────────────
        public void Update(
            BlType blType,
            int shipperCompanyId,
            int carrierCompanyId,
            int portOfLoadingId,
            int portOfDischargeId,
            string cargoDescription,
            decimal grossWeightMt,
            FreightTerms freightTerms,
            DateOnly issueDate,
            int? consigneeCompanyId = null,
            string? consigneeToOrder = null,
            int? notifyPartyCompanyId = null,
            string? hsCode = null,
            decimal? volumeM3 = null,
            int? packageCount = null,
            string? packageType = null,
            string? marksAndNumbers = null,
            string? freightAmount = null,
            string? incoterms = null,
            string? placeOfIssue = null,
            DateOnly? onBoardDate = null,
            int originalCopiesCount = 3,
            int? placeOfReceiptPortId = null,
            int? placeOfDeliveryPortId = null,
            string? notes = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(cargoDescription);
            if (grossWeightMt <= 0) throw new ArgumentException("GrossWeightMT must be positive.");
            if (shipperCompanyId <= 0) throw new ArgumentException("Invalid ShipperCompanyID.");
            if (carrierCompanyId <= 0) throw new ArgumentException("Invalid CarrierCompanyID.");

            if (blType == BlType.Straight && consigneeCompanyId == null)
                throw new InvalidOperationException("Straight BL requires a named Consignee.");

            if (blType == BlType.OrderBl && consigneeCompanyId == null && string.IsNullOrWhiteSpace(consigneeToOrder))
                throw new InvalidOperationException("Order BL requires either a Consignee or a 'To Order' instruction.");

            BlType = blType;
            ShipperCompanyID = shipperCompanyId;
            CarrierCompanyID = carrierCompanyId;
            ConsigneeCompanyID = consigneeCompanyId;
            ConsigneeToOrder = consigneeToOrder;
            NotifyPartyCompanyID = notifyPartyCompanyId;
            PortOfLoadingID = portOfLoadingId;
            PortOfDischargeID = portOfDischargeId;
            PlaceOfReceiptPortID = placeOfReceiptPortId;
            PlaceOfDeliveryPortID = placeOfDeliveryPortId;
            CargoDescription = cargoDescription;
            HsCode = hsCode;
            GrossWeightMT = grossWeightMt;
            VolumeM3 = volumeM3;
            PackageCount = packageCount;
            PackageType = packageType;
            MarksAndNumbers = marksAndNumbers;
            FreightTerms = freightTerms;
            FreightAmount = freightAmount;
            Incoterms = incoterms;
            IssueDate = issueDate;
            PlaceOfIssue = placeOfIssue;
            OnBoardDate = onBoardDate;
            OriginalCopiesCount = originalCopiesCount;
            Notes = notes;
            Touch();
        }

        // ── Methods الحساسة ──────────────────────────────────────────────────

        /// <summary>
        /// تغيير رقم البوليصة — له أثر على جميع المستندات المرتبطة
        /// </summary>
        public void ChangeBlNumber(string newBlNumber)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(newBlNumber);
            if (newBlNumber == BlNumber) return;
            BlNumber = newBlNumber;
            Touch();
        }

        /// <summary>
        /// تغيير نوع الإصدار — يغير طبيعة الوثيقة القانونية كلياً
        /// </summary>
        public void ChangeIssuanceType(BlIssuanceType newIssuanceType, int? masterBlId = null)
        {
            if (newIssuanceType == BlIssuanceType.House && masterBlId == null)
                throw new InvalidOperationException("House BL must reference a Master BL.");

            if (newIssuanceType == BlIssuanceType.Master)
                MasterBlID = null;
            else
                MasterBlID = masterBlId;

            IssuanceType = newIssuanceType;
            Touch();
        }

        /// <summary>
        /// ربط أو تغيير الـ Master BL المرجعي — للـ House BL فقط
        /// </summary>
        public void LinkToMasterBl(int masterBlId)
        {
            if (IssuanceType != BlIssuanceType.House)
                throw new InvalidOperationException("Only House BL can be linked to a Master BL.");
            if (masterBlId <= 0) throw new ArgumentException("Invalid MasterBlID.");
            MasterBlID = masterBlId;
            Touch();
        }
    }
}