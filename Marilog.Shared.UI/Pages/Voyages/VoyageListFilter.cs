using Marilog.Kernel.Enums;

namespace Marilog.Shared.UI.Pages.Voyages
{
    public sealed record VoyageListFilter(
        string? Search = null,
        int? VesselId = null,
        VoyageStatus? Status = null,
        DateOnly? Month = null);
}
