using Marilog.Domain.Common;
using Marilog.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Domain.Events
{
    public class ContractTerminatedEvent : IDomainEvent
    {
        public int Id { get; }
        public string Reason { get; }
        public ContractTerminatedEvent(int id, string reason)
        {
            Id = id;
            Reason = reason;
        }
    }
}
