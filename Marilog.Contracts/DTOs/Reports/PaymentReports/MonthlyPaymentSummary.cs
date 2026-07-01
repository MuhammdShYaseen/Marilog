using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Contracts.DTOs.Reports.PaymentReports
{
    public class MonthlyPaymentSummary
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal CashIn { get; set; }
        public decimal CashOut { get; set; }
        public decimal NetCashFlow { get; set; }
        public int Count { get; set; }
    }
}
