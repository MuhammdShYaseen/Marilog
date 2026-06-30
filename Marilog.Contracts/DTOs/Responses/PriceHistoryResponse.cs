using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Contracts.DTOs.Responses
{
    public class PriceHistoryResponse
    {
        public string? DocumentNumber { get; set; }
        public string? Supplier { get; set; }
        public string? Buyer { get; set; }
        public DateOnly DocDate { get; set; }
        public string? VesselName { get; set; }
        public string? ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public string? CurrencyCode { get; set; }
        public decimal Quantity { get; set; }
        public decimal? ChangePercent { get; set; }
        public decimal UnitPriceInBaseCurrency { get; set; }
        public string? CurrencyCodeBase { get; set; }
    }
}
