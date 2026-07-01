using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Contracts.DTOs.Reports.PaymentReports
{
    public class PaymentMethodSummary
    {
        public string? Method { get; set; }
        public decimal TotalBase { get; set; }
        public decimal CashIn { get; set; }
        public decimal CashOut { get; set; }
        public int Count { get; set; }
    }
}
