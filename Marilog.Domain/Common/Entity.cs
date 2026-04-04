using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Domain.Common
{
    public abstract class Entity
    {
        public int Id { get; protected set; }
        public Guid Guid { get; private set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; private set; }
        public bool IsDeleted { get; private set; } = false;
        public bool IsActive { get; protected set; } = true;

        private readonly List<IDomainEvent> _domainEvents = new();
        protected Entity()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }


        public void Delete()
        {
            IsDeleted = true;
            Touch();
        }

        public void Restore()
        {
            IsDeleted = false;
            Touch();
        }

        protected void Touch()
        {
            UpdatedAt = DateTime.UtcNow;
        }
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
        public void ClearDomainEvents() => _domainEvents.Clear();
        public void Deactivate() { IsActive = false; Touch(); }
        public virtual void Activate() { IsActive = true; Touch(); }
    }

    public interface IDomainEvent { }
}
