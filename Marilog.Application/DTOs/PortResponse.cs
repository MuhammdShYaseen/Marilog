using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.DTOs
{
    public class PortResponse
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public int CountryId { get; set; }
        public string? CountryName { get; set; }
        public bool IsActive { get; set; }
    }
}
