using Marilog.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Domain.Events
{
    public class ContractSuspendedEvent : IDomainEvent
    {
        public int Id { get; }
        public string Reason {  get; }
        public ContractSuspendedEvent(int id, string reason)
        {
            Id = id;
            Reason = reason;
            
        }

    }
}
