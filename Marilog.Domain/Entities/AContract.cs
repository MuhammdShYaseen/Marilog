

using Marilog.Domain.Common;
using Marilog.Domain.Events;
using Marilog.Domain.ValueObjects.Contract;

namespace Marilog.Domain.Entities
{
    public enum ContractType
    {
        CharterParty,   // عقد إيجار بحري
        CrewEmployment, // عقد توظيف بحار
        Supplier,       // عقد مورد
        Agency,         // عقد وكالة
    }
    public enum ContractStatus
    {
        Draft,
        Active,
        Expired,
        Terminated,
        Suspended,
    }
    public class AContract : Entity
    {
        public string ContractNumber { get; private protected set; } = null!;
        public ContractType Type { get; private protected set; }
        public ContractStatus Status { get; private protected set; }
        public DateOnly EffectiveDate { get; private protected set; }
        public DateOnly? ExpiryDate { get; private protected set; }
        public string? Notes { get; private protected set; }

        // ─── ملف العقد ───────────────────────────────────────────────────────
        public string? ContractFileUrl { get; private set; }
        public string? ContractFileName { get; private set; }

        private readonly List<ContractParty> _parties = [];
        public IReadOnlyCollection<ContractParty> Parties => _parties.AsReadOnly();

        // ─── Computed ─────────────────────────────────────────────────────────
        public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateOnly.FromDateTime(DateTime.UtcNow);

        // ─── Constructor ─────────────────────────────────────────────────────
        protected AContract() { }   // EF Core

        protected AContract(
            string contractNumber,
            ContractType type,
            DateOnly effectiveDate,
            DateOnly? expiryDate = null,
            string? notes = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(contractNumber);

            if (expiryDate.HasValue && expiryDate.Value <= effectiveDate)
                throw new ArgumentException("ExpiryDate must be after EffectiveDate.");

            ContractNumber = contractNumber;
            Type = type;
            Status = ContractStatus.Draft;
            EffectiveDate = effectiveDate;
            ExpiryDate = expiryDate;
            Notes = notes;
        }

        // ─── Party Management ─────────────────────────────────────────────────
        public void AddParty(int companyId, ContractRole role)
        {
            if (_parties.Any(p => p.CompanyId == companyId && p.Role == role))
                throw new ArgumentException($"Company {companyId} already has role {role} in this contract.");

            _parties.Add(new ContractParty(companyId, role));
        }

        public void RemoveParty(int companyId, ContractRole role)
        {
            var party = _parties.FirstOrDefault(p => p.CompanyId == companyId && p.Role == role)
                ?? throw new ArgumentException($"Party not found.");

            _parties.Remove(party);
        }

        public IReadOnlyList<int> GetCompanyIdsByRole(ContractRole role)
            => _parties
                .Where(p => p.Role == role)
                .Select(p => p.CompanyId)
                .ToList();

        // ─── Status Transitions ───────────────────────────────────────────────
        public override void Activate()
        {
            if (Status != ContractStatus.Draft)
                throw new ArgumentException("Only Draft contracts can be activated.");

            if (!_parties.Any())
                throw new ArgumentException("Cannot activate contract without parties.");

            Status = ContractStatus.Active;
            AddDomainEvent(new ContractActivatedEvent(Id, ContractNumber, Type));
        }

        public void Suspend(string reason)
        {
            if (Status != ContractStatus.Active)
                throw new ArgumentException("Only Active contracts can be suspended.");

            ArgumentException.ThrowIfNullOrWhiteSpace(reason);

            Status = ContractStatus.Suspended;
            AppendNote($"Suspended: {reason}");
            AddDomainEvent(new ContractSuspendedEvent(Id, reason));
        }

        public void Terminate(string reason)
        {
            if (Status is ContractStatus.Expired or ContractStatus.Terminated)
                throw new ArgumentException("Contract is already closed.");

            ArgumentException.ThrowIfNullOrWhiteSpace(reason);

            Status = ContractStatus.Terminated;
            AppendNote($"Terminated: {reason}");
            AddDomainEvent(new ContractTerminatedEvent(Id, reason));
        }

        public void MarkExpired()
        {
            if (Status != ContractStatus.Active)
                throw new ArgumentException("Only Active contracts can expire.");

            Status = ContractStatus.Expired;
            AddDomainEvent(new ContractExpiredEvent(Id));
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
        private void AppendNote(string note)
        {
            Notes = string.IsNullOrWhiteSpace(Notes)
                ? note
                : $"{Notes} | {note}";
        }
    }
}
