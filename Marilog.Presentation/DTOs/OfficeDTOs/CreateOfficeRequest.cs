namespace Marilog.Presentation.DTOs.OfficeDTOs
{
    public class CreateOfficeRequest
    {
        public string OfficeName { get; set; } = default!;
        public string City { get; set; } = default!;
        public int CountryId { get; set; }

        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? ContactName { get; set; }
    }
}
