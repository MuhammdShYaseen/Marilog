namespace Marilog.Presentation.DTOs.PortDTOs
{
    public class CreatePortRequest
    {
        public string PortCode { get; set; } = null!;
        public string PortName { get; set; } = null!;
        public int? CountryId { get; set; }
    }
}
