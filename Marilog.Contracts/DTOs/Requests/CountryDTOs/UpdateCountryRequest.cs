namespace Marilog.Contracts.DTOs.Requests.CountryDTOs
{
    public class UpdateCountryRequest
    {
        public string CountryCode { get; set; } = default!;
        public string CountryName { get; set; } = default!;
    }
}
