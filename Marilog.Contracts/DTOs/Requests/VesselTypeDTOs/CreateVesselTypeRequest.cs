namespace Marilog.Contracts.DTOs.Requests.VesselTypeDTOs
{
    public class CreateVesselTypeRequest
    {
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public int SortOrder { get; set; }
    }
}