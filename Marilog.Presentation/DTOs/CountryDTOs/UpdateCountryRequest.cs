namespace Marilog.Presentation.DTOs.CountryDTOs
{
    public class UpdateCountryRequest
    {
        public string CountryCode { get; set; } = default!;
        public string CountryName { get; set; } = default!;
    }
}
