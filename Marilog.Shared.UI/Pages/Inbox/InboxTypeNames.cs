using Marilog.Kernel.Enums;

namespace Marilog.Shared.UI.Pages.Inbox
{
    public static class InboxTypeNames
    {
        public static string Of(EntityType type) => type switch
        {
            EntityType.SwiftTransfer => "SWIFT transfer",
            EntityType.DocumentAdjustment => "Adjustment",
            EntityType.AContract => "Contract",
            EntityType.CrewContract => "Crew contract",
            EntityType.CrewPayroll => "Crew payroll",
            EntityType.BillOfLading => "Bill of lading",
            EntityType.InboxEntry => "Inbox entry",
            _ => type.ToString()
        };
    }
}