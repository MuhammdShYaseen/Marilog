using Marilog.Contracts.DTOs.Responses;
using Marilog.Kernel.Enums;

namespace Marilog.Shared.UI.Components.Comments
{
    // Snapshot of the signed-in user's unread comment counts, cascaded from the layout.
    // A new instance replaces the old one on every refresh so the UI re-renders.
    public sealed class UnreadComments
    {
        public static readonly UnreadComments Empty = new(Array.Empty<UnreadCommentCount>());

        private readonly Dictionary<(EntityType, int), int> _byRecord;
        private readonly Dictionary<EntityType, int> _byType;

        public UnreadComments(IEnumerable<UnreadCommentCount> items)
        {
            _byRecord = items.ToDictionary(x => (x.EntityType, x.EntityId), x => x.Count);
            _byType = _byRecord
                .GroupBy(x => x.Key.Item1)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Value));
        }

        public int ForRecord(EntityType type, int id)
            => _byRecord.TryGetValue((type, id), out var n) ? n : 0;

        public int ForTypes(IEnumerable<EntityType> types)
            => types.Distinct().Sum(t => _byType.TryGetValue(t, out var n) ? n : 0);
    }
}