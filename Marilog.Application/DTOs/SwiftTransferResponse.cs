using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.DTOs
{
    public class SwiftTransferResponse
    {
        public int Id { get; set; }
        public string SwiftReference { get; set; } = null!;
        public DateOnly TransactionDate { get; set; }

        public int CurrencyId { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;

        public decimal Amount { get; set; }
        public decimal AllocatedAmount { get; set; }
        public decimal UnallocatedAmount { get; set; }
        public bool IsFullyAllocated { get; set; }

        public int SenderCompanyId { get; set; }
        public string? SenderCompanyName { get; set; }
        public int ReceiverCompanyId { get; set; }
        public string? ReceiverCompanyName { get; set; }

        public string SenderBank { get; set; } = null!;
        public string ReceiverBank { get; set; } = null!;
        public string? PaymentReference { get; set; }
        public bool IsActive { get; set; }
    }
}
