namespace Marilog.Contracts.DTOs.Requests.VesselTypeDTOs
{
    /// <summary>Code is fixed after creation.</summary>
    public class UpdateVesselTypeRequest
    {
        public string Name { get; set; } = default!;
        public int SortOrder { get; set; }
    }
}