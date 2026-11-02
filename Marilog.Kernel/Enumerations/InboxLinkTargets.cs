using Marilog.Kernel.Enums;

namespace Marilog.Kernel.Enumerations
{
    public static class InboxLinkTargets
    {
        public static readonly IReadOnlyList<EntityType> All =
        [
            EntityType.Document,
            EntityType.SwiftTransfer,
            EntityType.Payment,
            EntityType.DocumentAdjustment,
            EntityType.AContract,
            EntityType.CrewContract,
            EntityType.CrewPayroll,
            EntityType.Voyage,
            EntityType.BillOfLading,
            EntityType.InboxEntry
        ];
    }
}
