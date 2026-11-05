using Marilog.Domain.Common;


namespace Marilog.Domain.Events
{
    public class ContractActivatedEvent : IDomainEvent
    {
        public int ID { get; }
        public string ContractNumber { get; }
        public int ContractTypeId { get; }
        public ContractActivatedEvent(int id, string contractNumber, int contractTypeId)
        {
            ID = id;
            ContractNumber = contractNumber;
            ContractTypeId = contractTypeId;

        }
    }
}
