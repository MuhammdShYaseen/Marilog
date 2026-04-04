using Marilog.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Marilog.Domain.Events
{
    public class ContractAmendedEvent : IDomainEvent
    {
        public ContractAmendedEvent(int id, int number, string description, DateOnly effectiveDate, string changedBy)
        {
            
        }
    }
}
