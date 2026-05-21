using Marilog.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.Interfaces.Events
{
    public interface IEventHandler<in TEvent> where TEvent : IDomainEvent
    {
        Task HandleAsync(TEvent @event, CancellationToken ct = default);
    }
}
