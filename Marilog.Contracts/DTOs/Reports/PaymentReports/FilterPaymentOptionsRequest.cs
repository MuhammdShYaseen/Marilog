using Marilog.Kernel.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Contracts.DTOs.Reports.PaymentReports
{
    // ─── Request DTO ────────────────────────────────────────────────────────
    public class FilterPaymentOptionsRequest
    {
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }

        public int? VesselId { get; set; }
        public int? SupplierId { get; set; }
        public int? BuyerId { get; set; }
        public int? VoyageId { get; set; }
        public int? DocTypeId { get; set; }

        public PaymentMethod? PaymentMethod { get; set; }
        public FinancialSide? Side { get; set; }

        // فلترة سريعة بديلة عن From/To
        public int? LastDays { get; set; }

        // إذا حابب تجيب دفعات SWIFT بس (مرتبطة بـ SwiftTransferId)
        public bool? SwiftOnly { get; set; }

        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
    }
}
