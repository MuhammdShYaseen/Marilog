namespace Marilog.Presentation.DTOs.PortDTOs
{
    public class UpdatePortRequest
    {
        public string PortCode { get; set; } = null!;
        public string PortName { get; set; } = null!;
        public int? CountryId { get; set; }
    }
}
