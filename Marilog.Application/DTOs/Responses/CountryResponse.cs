using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.DTOs.Responses
{
    public class CountryResponse
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public bool IsActive { get; set; }
    }
}
