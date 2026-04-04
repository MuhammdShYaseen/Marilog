using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Domain.ValueObjects.Contract
{
    public enum ContractRole
    {
        Owner,      // المالك
        Charterer,  // المستأجر
        Agent,      // الوكيل
        Supplier,   // المورد
        Guarantor,  // الضامن
    }
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
