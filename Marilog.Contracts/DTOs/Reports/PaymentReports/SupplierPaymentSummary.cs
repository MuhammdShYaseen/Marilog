using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Contracts.DTOs.Reports.PaymentReports
{
    public class SupplierPaymentSummary
    {
        public int SupplierId { get; set; }
        public decimal TotalPaidBase { get; set; }
        public string? SupplierName { get; set; }
        public int Count { get; set; }
    }
}
