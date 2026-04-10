namespace Marilog.Contracts.DTOs.Requests.PortDTOs
{
    public class CreatePortRequest
    {
        public string PortCode { get; set; } = null!;
        public string PortName { get; set; } = null!;
        public int? CountryId { get; set; }
    }
}
