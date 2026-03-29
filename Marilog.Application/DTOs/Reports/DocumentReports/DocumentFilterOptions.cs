using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.DTOs.Reports.DocumentReports
{
    public class DocumentFilterOptions
    {
        public int? SupplierId { get; set; }
        public int? BuyerId { get; set; }
        public int? VesselId { get; set; }
        public int? DocTypeId { get; set; }
        public bool UnpaidOnly { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
        public int? LastDays { get; set; }
    }
}
