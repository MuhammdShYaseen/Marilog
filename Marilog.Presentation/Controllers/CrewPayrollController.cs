using Marilog.Application.DTOs.Responses;
using Marilog.Application.Interfaces.Services;
using Marilog.Domain.Entities;
using Marilog.Presentation.DTOs.CrewPayrollDTOs;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers
{
    [ApiController]
    [Route("api/crew-payrolls")]
    public class CrewPayrollController : ControllerBase
    {
        private readonly ICrewPayrollService _service;

        public CrewPayrollController(ICrewPayrollService service)
        {
            _service = service;
        }

        // ─────────────────────────────────────────────
        // Queries
        // ─────────────────────────────────────────────

        [HttpGet("{id:int}")]
        public async Task<ActionResult<CrewPayrollResponse>> GetById(int id, CancellationToken ct)
        {
            var payroll = await _service.GetByIdAsync(id, ct);
            return payroll is null ? NotFound() : Ok(payroll);
        }

        [HttpGet("{id:int}/with-disbursements")]
        public async Task<ActionResult<CrewPayrollResponse>> GetWithDisbursements(int id, CancellationToken ct)
        {
            var payroll = await _service.GetWithDisbursementsAsync(id, ct);
            return payroll is null ? NotFound() : Ok(payroll);
        }

        [HttpGet("by-contract/{contractId:int}")]
        public async Task<ActionResult<IReadOnlyList<CrewPayrollResponse>>> GetByContract(int contractId, CancellationToken ct)
        {
            var payrolls = await _service.GetByContractAsync(contractId, ct);
            return Ok(payrolls);
        }

        [HttpGet("by-contract/{contractId:int}/month")]
        public async Task<ActionResult<CrewPayrollResponse>> GetByContractAndMonth(int contractId, [FromQuery] DateOnly month, CancellationToken ct)
        {
            var payroll = await _service.GetByContractAndMonthAsync(contractId, month, ct);
            return payroll is null ? NotFound() : Ok(payroll);
        }

        [HttpGet("by-month")]
        public async Task<ActionResult<IReadOnlyList<CrewPayrollResponse>>> GetByMonth([FromQuery] DateOnly month, CancellationToken ct)
        {
            var payrolls = await _service.GetByMonthAsync(month, ct);
            return Ok(payrolls);
        }

        [HttpGet("by-status/{status}")]
        public async Task<ActionResult<IReadOnlyList<CrewPayrollResponse>>> GetByStatus(PayrollStatus status, CancellationToken ct)
        {
            var payrolls = await _service.GetByStatusAsync(status, ct);
            return Ok(payrolls);
        }

        [HttpGet("outstanding")]
        public async Task<ActionResult<IReadOnlyList<CrewPayrollResponse>>> GetOutstanding(CancellationToken ct)
        {
            var payrolls = await _service.GetOutstandingAsync(ct);
            return Ok(payrolls);
        }

        // ─────────────────────────────────────────────
        // Commands
        // ─────────────────────────────────────────────

        [HttpPost]
        public async Task<ActionResult<CrewPayrollResponse>> Create([FromBody] CreateCrewPayrollRequest request, CancellationToken ct)
        {
            var payroll = await _service.CreateAsync(
                request.ContractId,
                request.PayrollMonth,
                request.Allowances,
                request.Deductions,
                request.Notes,
                ct);

            return CreatedAtAction(nameof(GetById), new { id = payroll.Id }, payroll);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCrewPayrollRequest request, CancellationToken ct)
        {
            await _service.UpdateAsync(id, request.WorkingDays, request.BasicWage,
                                       request.Allowances, request.Deductions,
                                       request.Notes, ct);
            return NoContent();
        }

        [HttpPatch("{id:int}/approve")]
        public async Task<IActionResult> Approve(int id, CancellationToken ct)
        {
            await _service.ApproveAsync(id, ct);
            return NoContent();
        }

        [HttpPatch("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id, [FromBody] CancelPayrollRequest request, CancellationToken ct)
        {
            await _service.CancelAsync(id, request.Reason, ct);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await _service.DeleteAsync(id, ct);
            return NoContent();
        }

        // ─────────────────────────────────────────────
        // Disbursements
        // ─────────────────────────────────────────────

        [HttpPost("{payrollId:int}/pay-cash")]
        public async Task<ActionResult<CrewPayrollDisbursement>> PayCash(int payrollId, [FromBody] PayCashRequest request, CancellationToken ct)
        {
            var disbursement = await _service.PayCashAsync(payrollId, request.VoyageId, request.Amount,
                                                           request.PaidOn, request.Notes, ct);
            return Ok(disbursement);
        }

        [HttpPost("{payrollId:int}/pay-office")]
        public async Task<ActionResult<CrewPayrollDisbursement>> PayAtOffice(int payrollId, [FromBody] PayAtOfficeRequest request, CancellationToken ct)
        {
            var disbursement = await _service.PayAtOfficeAsync(payrollId, request.OfficeId,
                                                               request.Amount, request.PaidOn,
                                                               request.RecipientName,
                                                               request.RecipientIdNumber,
                                                               request.Notes, ct);
            return Ok(disbursement);
        }

        [HttpPost("{payrollId:int}/pay-bank-transfer")]
        public async Task<ActionResult<CrewPayrollDisbursement>> PayByBankTransfer(int payrollId, [FromBody] PayByBankTransferRequest request, CancellationToken ct)
        {
            var disbursement = await _service.PayByBankTransferAsync(payrollId,
                                                                     request.SwiftTransferId,
                                                                     request.Amount,
                                                                     request.PaidOn,
                                                                     request.Notes, ct);
            return Ok(disbursement);
        }

        [HttpPatch("{payrollId:int}/disbursements/{disbursementId:int}/confirm")]
        public async Task<IActionResult> ConfirmDisbursement(int payrollId, int disbursementId, CancellationToken ct)
        {
            await _service.ConfirmDisbursementAsync(payrollId, disbursementId, ct);
            return NoContent();
        }

        [HttpPatch("{payrollId:int}/disbursements/{disbursementId:int}/cancel")]
        public async Task<IActionResult> CancelDisbursement(int payrollId, int disbursementId, [FromBody] CancelDisbursementRequest request, CancellationToken ct)
        {
            await _service.CancelDisbursementAsync(payrollId, disbursementId, request.Reason, ct);
            return NoContent();
        }
    }
}
