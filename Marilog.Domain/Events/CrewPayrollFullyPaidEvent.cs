using Marilog.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Domain.Events
{
    public class CrewPayrollFullyPaidEvent : IDomainEvent
    {//Id, ContractId

        public int Id { get; }
        public int ContractId { get; }
        public CrewPayrollFullyPaidEvent(int id, int contractId)
        {
            Id = id;
            ContractId = contractId;
        }
    }
}
