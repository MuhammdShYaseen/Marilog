using Marilog.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Domain.Events
{
    
    public class DocumentFullyPaidEvent : IDomainEvent
    {
        public int DocumentId { get; }
        public DocumentFullyPaidEvent(int docId)
        {
            DocumentId = docId;
        }
    }

}
