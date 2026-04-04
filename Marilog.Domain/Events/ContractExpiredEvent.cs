using Marilog.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Domain.Events
{
    public class ContractExpiredEvent : IDomainEvent
    {
        public int Id { get; }
        public ContractExpiredEvent(int id)
        {
            Id = id;
        }
    }
}
