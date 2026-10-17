using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Shared.UI.Components.Fleet
{
    public sealed record FleetVessel(int VesselId, string VesselName, VesselPositionResponse Position);
}