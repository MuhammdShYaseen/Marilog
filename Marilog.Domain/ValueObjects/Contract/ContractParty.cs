namespace Marilog.Domain.ValueObjects.Contract
{
    public sealed class ContractParty
    {
        public int CompanyId { get; }
        public int RoleId { get; }     // FK → ContractRoles

        private ContractParty() { }   // EF Core

        internal ContractParty(int companyId, int roleId)
        {
            CompanyId = companyId;
            RoleId = roleId;
        }
    }
}
