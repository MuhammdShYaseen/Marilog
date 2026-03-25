using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.DTOs
{
    public class CompanyResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? RegistrationNumber { get; set; }  // حقل فارغ كما في المثال
        public string? ContactName { get; set; } = null!;
        public string? Email { get; set; } = null!;
        public string? Phone { get; set; } = null!;
        public string? Address { get; set; } = null!;
        public bool IsActive { get; set; }
        public List<VesselResponse> Vessels { get; set; } = new List<VesselResponse>();
    }
}
