using Marilog.Domain.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;

namespace Marilog.Domain.Events
{   //Id, ContractId, GrossAmount
    
    public class CrewPayrollApprovedEvent : IDomainEvent
    {
        public int Id { get; }
        public int ContractId { get; }
        public decimal GrossAmount { get; }
        public CrewPayrollApprovedEvent(int id, int contractId, decimal grossAmount)
        {
            Id = id;
            ContractId = contractId;
            GrossAmount = grossAmount;
        }
    }
}
