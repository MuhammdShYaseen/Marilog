

namespace Marilog.Shared.UI.Pages.Voyages
{
    namespace Marilog.Shared.UI.Pages.Voyages
    {
        public sealed record MasterOption(
            int ContractId,
            string Name,
            string? Rank,
            bool IsCurrentVesselMaster);
    }
}
