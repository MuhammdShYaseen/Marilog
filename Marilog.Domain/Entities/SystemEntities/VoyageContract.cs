namespace Marilog.Domain.Entities.SystemEntities
{
    /// <summary>
    /// Link: the voyage is performed under this contract (CP, TC, COA...).
    /// Many-to-many: a TC/COA covers many voyages; a voyage can run under several contracts.
    /// </summary>
    public sealed class VoyageContract
    {
        public int ContractID { get; private set; }

        private VoyageContract() { }   // EF Core

        internal VoyageContract(int contractId) => ContractID = contractId;
    }
}