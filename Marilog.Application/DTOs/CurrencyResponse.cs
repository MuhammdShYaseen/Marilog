using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.DTOs
{
    public class CurrencyResponse
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Symbol { get; set; } = null!;
        public decimal ExchangeRate { get; set; }
        public bool IsBaseCurrency { get; set; }
        public bool IsActive { get; set; }
    }
}
