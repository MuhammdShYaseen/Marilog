using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.DTOs
{
    public class DocumentItemResponse
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = null!;
        public decimal Quantity { get; set; }
        public string? Unit { get; set; } = null!;
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}
