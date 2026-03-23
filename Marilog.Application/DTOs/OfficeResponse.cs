using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.DTOs
{
    public class OfficeResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string City { get; set; } = null!;
        public int CountryId { get; set; }
        public string? CountryName { get; set; }
        public string Address { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string ContactName { get; set; } = null!;
        public bool IsActive { get; set; }
    }
}
