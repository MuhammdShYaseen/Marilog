using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.DTOs.Commands.CrewPayroll
{
    public record CreateCrewPayrollCommand(
     int ContractId,
     DateOnly PayrollMonth,
     decimal Allowances = 0m,
     decimal Deductions = 0m,
     string? Notes = null
 );
}
