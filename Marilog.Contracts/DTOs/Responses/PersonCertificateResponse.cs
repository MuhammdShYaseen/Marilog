using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Contracts.DTOs.Responses
{
    public class PersonCertificateResponse
    {
        public int Index { get; set; }
        public string CertificateName { get; set; } = string.Empty;
        public DateOnly? IssueDate { get; set; }
        public DateOnly? ExpiryDate { get; set; }
        public string? Description { get; set; }
    }
}
