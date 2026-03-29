using Marilog.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.DTOs.Responses
{
    public class DisbursementResponse
    {
        public int Id { get; set; }
        public PaymentMethod Method { get; set; }
        public decimal Amount { get; set; }
        public DateOnly PaidOn { get; set; }
        public DisbursementStatus? Status { get; set; } = null!;
        public int? VoyageId { get; set; }
        public string? VoyageNumber { get; set; }
        public int? OfficeId { get; set; }
        public string? OfficeName { get; set; }
        public string? RecipientName { get; set; } = null!;
        public string? RecipientIdNumber { get; set; } = null!;
        public int? SwiftTransferId { get; set; }
        public string? SwiftReference { get; set; }
        public string? Notes { get; set; }
        public string? CancelReason { get; set; }
    }
}
