using Marilog.Domain.Common;
using Marilog.Domain.Entities.LaytimeEntities;
using Marilog.Domain.Events;
using Marilog.Domain.ValueObjects.Contract;
using Marilog.Kernel.Enumerations;
using Marilog.Kernel.Primitives;

namespace Marilog.Domain.Entities.SystemEntities
{
    public class AContract : Entity
    {
        // ─── Properties ───────────────────────────────────────────────────────
        public string ContractNumber { get; private set; } = null!;
        public int ContractTypeId { get; private set; }
        public ContractType ContractType { get; private set; } = null!;
        public ContractStatus Status { get; private set; } = null!;
        public DateOnly EffectiveDate { get; private set; }
        public DateOnly? ExpiryDate { get; private set; }
        public string? Notes { get; private set; }

        private readonly List<ContractParty> _parties = [];
        private readonly List<ContractAmendment> _amendments = [];
        public IReadOnlyCollection<ContractParty> Parties => _parties.AsReadOnly();
        public IReadOnlyCollection<ContractAmendment> Amendments => _amendments.AsReadOnly();

        // 1:1 — created and edited by CharterTermsService (CharterTerms.ContractId)
        private CharterTerms? _charterTerms;
        public CharterTerms? CharterTerms => _charterTerms;

        // ─── Computed ─────────────────────────────────────────────────────────
        public bool IsExpiredAsOf(DateOnly today)
            => ExpiryDate.HasValue && ExpiryDate.Value < today;

        // ─── Constructors ─────────────────────────────────────────────────────
        protected AContract() { }   // EF Core

        public static AContract Create(string contractNumber, int contractTypeId, DateOnly effectiveDate,
            DateOnly? expiryDate = null, string? notes = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(contractNumber);

            if (contractTypeId <= 0)
                throw new ArgumentException("Contract type is required.");

            if (expiryDate.HasValue && expiryDate.Value <= effectiveDate)
                throw new ArgumentException("Expiry date must be after the effective date.");

            return new AContract
            {
                ContractNumber = contractNumber.Trim(),
                ContractTypeId = contractTypeId,
                Status = ContractStatus.Draft,
                EffectiveDate = effectiveDate,
                ExpiryDate = expiryDate,
                Notes = notes
            };
        }

        /// <summary>
        /// Expiry date is editable here only while Draft — after that use <see cref="ChangeExpiry"/> (records an amendment).
        /// </summary>
        public Result Update(string contractNumber, int contractTypeId, DateOnly effectiveDate,
            DateOnly? expiryDate, string? notes)
        {
            if (Status.IsClosed)
                return Result.Fail($"Cannot edit a contract in '{Status}' status.");

            if (string.IsNullOrWhiteSpace(contractNumber))
                return Result.Fail("Contract number is required.");

            if (contractTypeId <= 0)
                return Result.Fail("Contract type is required.");

            if (expiryDate.HasValue && expiryDate.Value <= effectiveDate)
                return Result.Fail("Expiry date must be after the effective date.");

            if (Status != ContractStatus.Draft && expiryDate != ExpiryDate)
                return Result.Fail("Use 'Change expiry' to change the expiry date of a non-draft contract.");

            ContractNumber = contractNumber.Trim();
            ContractTypeId = contractTypeId;
            EffectiveDate = effectiveDate;
            ExpiryDate = expiryDate;
            Notes = notes;
            Touch();
            return Result.Ok();
        }

        // ─── Party Management ─────────────────────────────────────────────────
        // Draft: plain add/remove. Active/Suspended: the change is recorded as an amendment automatically.

        public Result AddParty(int companyId, string companyName, int roleId, string roleName, string changedBy, DateTime nowUtc)
        {
            if (Status.IsClosed)
                return Result.Fail($"Cannot add a party to a contract in '{Status}' status.");

            if (_parties.Any(p => p.CompanyId == companyId && p.RoleId == roleId))
                return Result.Fail($"{companyName} is already '{roleName}' in this contract.");

            _parties.Add(new ContractParty(companyId, roleId));

            if (Status != ContractStatus.Draft)
                AddAmendment($"Added party: {companyName} ({roleName})", DateOnly.FromDateTime(nowUtc), changedBy, nowUtc);

            Touch();
            return Result.Ok();
        }

        public Result RemoveParty(int companyId, string companyName, int roleId, string roleName, string changedBy, DateTime nowUtc)
        {
            if (Status.IsClosed)
                return Result.Fail($"Cannot remove a party from a contract in '{Status}' status.");

            var party = _parties.FirstOrDefault(p => p.CompanyId == companyId && p.RoleId == roleId);
            if (party is null)
                return Result.Fail($"Party {companyName} ({roleName}) not found.");

            if (_parties.Count == 1 && Status != ContractStatus.Draft)
                return Result.Fail("Cannot remove the last party from a non-draft contract.");

            _parties.Remove(party);

            if (Status != ContractStatus.Draft)
                AddAmendment($"Removed party: {companyName} ({roleName})", DateOnly.FromDateTime(nowUtc), changedBy, nowUtc);

            Touch();
            return Result.Ok();
        }

        // ─── Status Transitions ───────────────────────────────────────────────

        public Result Activate(DateOnly today)
        {
            if (!Status.CanBeActivated)
                return Result.Fail($"Cannot activate a contract in '{Status}' status.");

            if (_parties.Count == 0)
                return Result.Fail("Cannot activate a contract without parties.");

            if (IsExpiredAsOf(today))
                return Result.Fail($"Cannot activate contract '{ContractNumber}': it expired on {ExpiryDate}.");

            Status = ContractStatus.Active;
            Touch();
            AddDomainEvent(new ContractActivatedEvent(Id, ContractNumber, ContractTypeId));
            return Result.Ok();
        }

        public Result Suspend(string reason)
        {
            if (!Status.CanBeSuspended)
                return Result.Fail($"Cannot suspend a contract in '{Status}' status.");

            if (string.IsNullOrWhiteSpace(reason))
                return Result.Fail("Suspension reason is required.");

            Status = ContractStatus.Suspended;
            AppendNote($"Suspended: {reason.Trim()}");
            Touch();
            AddDomainEvent(new ContractSuspendedEvent(Id, reason));
            return Result.Ok();
        }

        public Result Terminate(string reason)
        {
            if (!Status.CanBeTerminated)
                return Result.Fail($"Cannot terminate a contract in '{Status}' status.");

            if (string.IsNullOrWhiteSpace(reason))
                return Result.Fail("Termination reason is required.");

            Status = ContractStatus.Terminated;
            AppendNote($"Terminated: {reason.Trim()}");
            Touch();
            AddDomainEvent(new ContractTerminatedEvent(Id, reason));
            return Result.Ok();
        }

        public Result MarkExpired(DateOnly today)
        {
            if (Status.IsClosed)
                return Result.Fail($"Cannot expire a contract in '{Status}' status.");

            if (!IsExpiredAsOf(today))
                return Result.Fail("Contract has not reached its expiry date yet.");

            Status = ContractStatus.Expired;
            Touch();
            AddDomainEvent(new ContractExpiredEvent(Id));
            return Result.Ok();
        }

        // ─── Amendments ───────────────────────────────────────────────────────

        public Result RecordAmendment(string description, DateOnly effectiveDate, string changedBy, DateTime nowUtc)
        {
            if (Status.IsClosed)
                return Result.Fail($"Cannot amend a contract in '{Status}' status.");

            if (string.IsNullOrWhiteSpace(description))
                return Result.Fail("Amendment description is required.");

            var amendment = AddAmendment(description.Trim(), effectiveDate, changedBy, nowUtc);
            Touch();
            AddDomainEvent(new ContractAmendedEvent(Id, amendment.AmendmentNumber, amendment.Description, effectiveDate, changedBy));
            return Result.Ok();
        }

        public Result ChangeExpiry(DateOnly newExpiryDate, string changedBy, DateTime nowUtc)
        {
            if (Status.IsClosed)
                return Result.Fail($"Cannot change the expiry date of a contract in '{Status}' status.");

            if (newExpiryDate <= EffectiveDate)
                return Result.Fail("Expiry date must be after the effective date.");

            if (ExpiryDate == newExpiryDate)
                return Result.Fail("The new expiry date is the same as the current one.");

            var description = ExpiryDate is null
                ? $"Set expiry date to {newExpiryDate:dd MMM yyyy}"
                : newExpiryDate > ExpiryDate.Value
                    ? $"Extended expiry date from {ExpiryDate.Value:dd MMM yyyy} to {newExpiryDate:dd MMM yyyy}"
                    : $"Reduced expiry date from {ExpiryDate.Value:dd MMM yyyy} to {newExpiryDate:dd MMM yyyy}";

            var amendment = AddAmendment(description, DateOnly.FromDateTime(nowUtc), changedBy, nowUtc);
            ExpiryDate = newExpiryDate;
            Touch();
            AddDomainEvent(new ContractExpiryExtendedEvent(Id, ContractNumber, newExpiryDate, amendment.AmendmentNumber));
            return Result.Ok();
        }

        // ─── Private ──────────────────────────────────────────────────────────

        private ContractAmendment AddAmendment(string description, DateOnly effectiveDate, string changedBy, DateTime recordedAtUtc)
        {
            var amendment = new ContractAmendment(description, effectiveDate, changedBy, recordedAtUtc);
            _amendments.Add(amendment);
            return amendment;
        }

        private void AppendNote(string note) =>
            Notes = string.IsNullOrWhiteSpace(Notes) ? note : $"{Notes} | {note}";
    }
}
