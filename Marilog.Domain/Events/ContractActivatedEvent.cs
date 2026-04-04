using Marilog.Domain.Common;
using Marilog.Domain.Enumerations;


namespace Marilog.Domain.Events
{
    public class ContractActivatedEvent : IDomainEvent
    {
        public int ID { get; }
        public string ContractNumber { get; }
        public ContractType Type { get; }
        public ContractActivatedEvent(int id, string contractNumber, ContractType type)
        {
            ID = id;
            ContractNumber = contractNumber;
            Type = type;
            
        }
    }
}
