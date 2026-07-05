
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Kernel.Enums;

namespace Marilog.Domain.ValueObjects.Contract
{
    
    public sealed class ContractParty
    {
        public int CompanyId { get; }
        public ContractRole Role { get; }

        private ContractParty() { }   // EF Core

        internal ContractParty(int companyId, ContractRole role)
        {
            CompanyId = companyId;
            Role = role;
        }
        public override string ToString()
        {
            return Role.ToString();
        }
    }
}
