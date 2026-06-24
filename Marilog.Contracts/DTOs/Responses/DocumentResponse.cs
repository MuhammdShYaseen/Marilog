using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Responses
{
    public class DocumentResponse
    {
        public int Id { get; set; }
        public string DocNumber { get; set; } = null!;
        public int DocTypeId { get; set; }
        public string? DocTypeName { get; set; } = string.Empty;
        public DateOnly DocDate { get; set; }

        public int? SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public int? BuyerId { get; set; }
        public string? BuyerName { get; set; }
        public int? VesselId { get; set; }
        public string? VesselName { get; set; }
        public int? PortId { get; set; }
        public string? PortName { get; set; }
        public int CurrencyId { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal RemainingBalance { get; set; }
        public bool IsFullyPaid { get; set; }

        public string? Reference { get; set; }
        public int? ParentDocumentId { get; set; }
        public bool IsActive { get; set; }

        public List<DocumentItemResponse> Items { get; set; } = new List<DocumentItemResponse>();
        public List<PaymentResponse> Payments { get; set; } = new List<PaymentResponse>();
        public decimal? PaidAmount { get;  set; }
        public decimal Remaining { get;  set; }
        public decimal TotalItemsAmount {  get; set; }
        public bool Is_TotalAmount_Equal_TotalItemsAmount {  get; set; }
        public decimal TotalAmount_Minus_TotalItemsAmount { get; set; }
        public decimal TotalAmountBase { get; set; }
        public decimal PaidAmountBase { get; set; }
        public decimal RemainingBase { get; set; }
        public string CurrencyCodeBase { get; set; } = string.Empty;
        public string CurrencyNameBase { get; set; } = string.Empty;
        public FinancialSide Side { get; set; }


        /// <summary>عمق الـ node في الشجرة — الجذر = 0</summary>
        public int Depth { get; set; }

        /// <summary>الأبناء المباشرون لهذا المستند</summary>
        public List<DocumentResponse> Children { get; set; } = [];

        /// <summary>هل عنده أبناء</summary>
        public bool HasChildren => Children.Count > 0;
    }
}
