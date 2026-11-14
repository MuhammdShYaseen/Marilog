using Marilog.Domain.Common;
using Marilog.Kernel.Enums;

namespace Marilog.Domain.Entities.SystemEntities
{
    public class InboxEntry : Entity
    {
        public InboxDirection Direction { get; private set; }   // fixed once created: the number depends on it
        public int Year { get; private set; }
        public int Sequence { get; private set; }
        public string Number { get; private set; } = null!;
        public DateOnly ReceivedDate { get; private set; }
        public InboxSource Source { get; private set; }
        public List<int> VesselIds { get; private set; } = [];
        public string Description { get; private set; } = null!;
        public bool IsProcessed { get; private set; }
        public bool IsCancelled { get; private set; }

        private InboxEntry() { }

        public static InboxEntry Create(InboxDirection direction, int year, int sequence, string number, DateOnly receivedDate,
            InboxSource source, List<int> vesselIds, string description)
            => new()
            {
                Direction = direction,
                Year = year,
                Sequence = sequence,
                Number = number,
                ReceivedDate = receivedDate,
                Source = source,
                VesselIds = vesselIds?.Distinct().ToList() ?? [],
                Description = description
            };

        public void Update(DateOnly receivedDate, InboxSource source, List<int> vesselIds, string description)
        {
            ReceivedDate = receivedDate;
            Source = source;
            VesselIds = vesselIds?.Distinct().ToList() ?? [];
            Description = description;
        }

        public void ChangeSequence(int sequence, string number)
        {
            Sequence = sequence;
            Number = number;
        }

        public void MarkProcessed() => IsProcessed = true;

        public void Cancel() => IsCancelled = true;
        public void Reopen() => IsProcessed = false;
    }
}