using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.DTOs.Responses
{
    public class DocumentResponse
    {
        public int Id { get; set; }
        public string DocNumber { get; set; } = null!;
        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; } = string.Empty;
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
        public string? FilePath { get; set; }
        public int? ParentDocumentId { get; set; }
        public bool IsActive { get; set; }

        public List<DocumentItemResponse> Items { get; set; } = new List<DocumentItemResponse>();
        public List<PaymentResponse> Payments { get; set; } = new List<PaymentResponse>();
    }
}
