namespace Marilog.Presentation.DTOs.CountryDTOs
{
    public class CreateCountryRequest
    {
        public string CountryCode { get; set; } = default!;
        public string CountryName { get; set; } = default!;
    }
}
