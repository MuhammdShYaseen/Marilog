using Marilog.Domain.ValueObjects.Contract;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.DTOs.Responses
{
    public class ContractPartyResponse
    {
        public int CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public string Role {  get; set; } = string.Empty;
    }
}
