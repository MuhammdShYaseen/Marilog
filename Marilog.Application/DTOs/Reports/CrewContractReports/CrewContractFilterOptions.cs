using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.DTOs.Reports.CrewContractReports
{
    public class CrewContractFilterOptions
    {
        public int? VesselId { get; set; }
        public int? PersonId { get; set; }
        public bool? IsActive { get; set; }
        public string? RankCode { get; set; }          // مثال: "MASTER"
        public DateOnly? OnDate { get; set; }         // لفلترة العقود الفعالة في تاريخ معين
        public bool? OnlyMasters { get; set; }        // يمكن استخدامه بدلاً من RankCode
    }
}
