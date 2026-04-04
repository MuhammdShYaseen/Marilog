using Marilog.Domain.Common;
using Marilog.Domain.Enumerations;
using Marilog.Domain.Events;
using Marilog.Domain.ValueObjects.Contract;

namespace Marilog.Domain.Entities
{
    public class AContract : Entity
    {
        // ─── Properties ───────────────────────────────────────────────────────
        public string ContractNumber { get; private set; } = null!;
        public ContractType Type { get; private set; } = null!;
        public ContractStatus Status { get; private set; } = null!;
        public DateOnly EffectiveDate { get; private set; }
        public DateOnly? ExpiryDate { get; private set; }
        public string? Notes { get; private set; }
        public string? ContractFileUrl { get; private set; }
        public string? ContractFileName { get; private set; }

        private readonly List<ContractParty> _parties = [];
        private readonly List<ContractAmendment> _amendments = [];

        public IReadOnlyCollection<ContractParty> Parties => _parties.AsReadOnly();
        public IReadOnlyCollection<ContractAmendment> Amendments => _amendments.AsReadOnly();

        // ─── Computed ─────────────────────────────────────────────────────────
        public bool IsExpiredAsOf(DateOnly today)
            => ExpiryDate.HasValue && ExpiryDate.Value < today;

        // ─── Constructors ─────────────────────────────────────────────────────
        protected AContract() { }   // EF Core

        public static AContract Create(string contractNumber, ContractType type, DateOnly effectiveDate, DateOnly? expiryDate = null, string? notes = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(contractNumber);

            if (!Enum.IsDefined(typeof(ContractType), type))
                throw new ArgumentException("Invalid contract type.");

            if (expiryDate.HasValue && expiryDate.Value <= effectiveDate)
                throw new ArgumentException("ExpiryDate must be after EffectiveDate.");
            return new AContract
            {
                ContractNumber = contractNumber,
                Type = type,
                Status = ContractStatus.Draft,
                EffectiveDate = effectiveDate,
                ExpiryDate = expiryDate,
                Notes = notes
            };
            
        }

        // ─── Party Management ─────────────────────────────────────────────────

        public Result AddParty(int companyId, ContractRole role)
        {
            if (_parties.Any(p => p.CompanyId == companyId && p.Role == role))
                return Result.Fail(
                    $"Company {companyId} already holds role '{role}' in this contract.");

            _parties.Add(new ContractParty(companyId, role));
            return Result.Ok();
        }

        /// <summary>
        /// الحذف في حالة Draft فقط — لا يسمح بالحذف من عقد Active.
        /// لحذف طرف من عقد Active استخدم <see cref="RemovePartyViaAmendment"/>.
        /// </summary>
        public Result RemoveParty(int companyId, ContractRole role)
        {
            var party = _parties.FirstOrDefault(p => p.CompanyId == companyId && p.Role == role);
            if (party is null)
                return Result.Fail($"Party (company={companyId}, role={role}) not found.");

            if (Status != ContractStatus.Draft)
                return Result.Fail(
                    $"Cannot remove a party from a contract in '{Status}' status. " +
                    "Use RemovePartyViaAmendment for Active contracts.");

            if (_parties.Count == 1)
                return Result.Fail("Cannot remove the last party from the contract.");

            _parties.Remove(party);
            return Result.Ok();
        }

        /// <summary>
        /// الحذف الرسمي من عقد Active — يشترط Amendment مسجَّل مسبقاً.
        /// </summary>
        public Result RemovePartyViaAmendment(int companyId, ContractRole role, int amendmentNumber)
        {
            var amendment = _amendments.FirstOrDefault(a => a.AmendmentNumber == amendmentNumber);
            if (amendment is null)
                return Result.Fail($"Amendment #{amendmentNumber} not found.");

            var party = _parties.FirstOrDefault(p => p.CompanyId == companyId && p.Role == role);
            if (party is null)
                return Result.Fail($"Party (company={companyId}, role={role}) not found.");

            if (_parties.Count == 1)
                return Result.Fail("Cannot remove the last party from the contract.");

            _parties.Remove(party);
            AppendNote($"Party (company={companyId}, role={role}) removed via Amendment #{amendmentNumber}.");
            return Result.Ok();
        }

        // ─── Status Transitions ───────────────────────────────────────────────

        public Result Activate(DateOnly today)
        {
            if (!Status.CanBeActivated)
                return Result.Fail($"Cannot activate a contract in '{Status}' status.");

            if (_parties.Count == 0)
                return Result.Fail("Cannot activate contract without parties.");

            if (IsExpiredAsOf(today))
                return Result.Fail(
                    $"Cannot activate contract '{ContractNumber}': it expired on {ExpiryDate}.");

            Status = ContractStatus.Active;
            AddDomainEvent(new ContractActivatedEvent(Id, ContractNumber, Type));
            return Result.Ok();
        }

        public Result Suspend(string reason)
        {
            if (!Status.CanBeSuspended)
                return Result.Fail($"Cannot suspend a contract in '{Status}' status.");

            if (string.IsNullOrWhiteSpace(reason))
                return Result.Fail("Suspension reason is required.");

            Status = ContractStatus.Suspended;
            AppendNote($"Suspended: {reason}");
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
            AppendNote($"Terminated: {reason}");
            AddDomainEvent(new ContractTerminatedEvent(Id, reason));
            return Result.Ok();
        }

        public Result MarkExpired(DateOnly today)
        {
            if (!IsExpiredAsOf(today))
                return Result.Fail("Contract has not reached its expiry date yet.");

            if (Status.IsClosed)
                return Result.Fail($"Cannot expire a contract in '{Status}' status.");

            Status = ContractStatus.Expired;
            AddDomainEvent(new ContractExpiredEvent(Id));
            return Result.Ok();
        }

        // ─── Amendment ────────────────────────────────────────────────────────

        /// <summary>
        /// يُسجِّل تعديلاً رسمياً على العقد دون تغيير الحالة.
        /// التغييرات الفعلية (ExpiryDate / Parties) تتم عبر دوال مخصصة تستلزم رقم التعديل.
        /// </summary>
        public Result RecordAmendment(
            string description,
            DateOnly effectiveDate,
            string changedBy,
            DateOnly today,
            DateTime recordedAtUtc)   // يُمرَّر من Application Layer عبر IDateTimeProvider
        {
            if (Status.IsClosed)
                return Result.Fail($"Cannot amend a contract in '{Status}' status.");

            if (string.IsNullOrWhiteSpace(description))
                return Result.Fail("Amendment description is required.");

            if (string.IsNullOrWhiteSpace(changedBy))
                return Result.Fail("ChangedBy is required.");

            if (effectiveDate < today)
                return Result.Fail("Amendment effective date cannot be in the past.");

            var number = _amendments.Count + 1;

            var amendment = new ContractAmendment(
                amendmentNumber: number,
                description: description,
                effectiveDate: effectiveDate,
                changedBy: changedBy,
                recordedAtUtc: recordedAtUtc);

            _amendments.Add(amendment);
            AppendNote($"Amendment #{number}: {description}");
            AddDomainEvent(new ContractAmendedEvent(Id, number, description, effectiveDate, changedBy));
            return Result.Ok();
        }

        /// <summary>
        /// تمديد تاريخ الانتهاء — يشترط ربطه بـ Amendment مسجَّل مسبقاً.
        /// </summary>
        public Result ExtendExpiry(DateOnly newExpiryDate, int amendmentNumber, DateOnly today)
        {
            if (Status.IsClosed)
                return Result.Fail("Cannot extend a closed contract.");

            var amendment = _amendments.FirstOrDefault(a => a.AmendmentNumber == amendmentNumber);
            if (amendment is null)
                return Result.Fail($"Amendment #{amendmentNumber} not found.");

            if (ExpiryDate.HasValue && newExpiryDate <= ExpiryDate.Value)
                return Result.Fail($"New expiry must be after current expiry ({ExpiryDate}).");

            if (newExpiryDate <= today)
                return Result.Fail("New expiry date must be in the future.");

            ExpiryDate = newExpiryDate;
            AddDomainEvent(new ContractExpiryExtendedEvent(Id, ContractNumber, newExpiryDate, amendmentNumber));
            return Result.Ok();
        }

        // ─── File Attachment ──────────────────────────────────────────────────

        public void AttachFile(string fileUrl, string fileName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(fileUrl);
            ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

            ContractFileUrl = fileUrl;
            ContractFileName = fileName;
        }

        // ─── Private Helpers ──────────────────────────────────────────────────

        private void AppendNote(string note) =>
            Notes = string.IsNullOrWhiteSpace(Notes) ? note : $"{Notes} | {note}";
    }
}