using Marilog.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.DTOs.Reports.CrewPayrollReports
{
    public class CrewPayrollFilterOptions
    {
        public int? ContractId { get; set; }
        public int? PersonId { get; set; }
        public int? VesselId { get; set; }
        public DateOnly? Month { get; set; }                  // فلترة حسب الشهر
        public PayrollStatus? Status { get; set; }            // فلترة حسب الحالة
        public bool OnlyOutstanding { get; set; } = false;    // للمتبقي
        public bool IncludeDisbursements { get; set; } = false; // لدفعات تفصيلية
    }
}
