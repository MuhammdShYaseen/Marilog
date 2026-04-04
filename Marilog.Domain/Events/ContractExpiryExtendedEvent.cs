using Marilog.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Domain.Events
{
    public class ContractExpiryExtendedEvent : IDomainEvent
    {

        public ContractExpiryExtendedEvent(int id, string contractNumber, DateOnly newExpiryDate, int amendmentNumber)
        {
            
        }
    }
}
