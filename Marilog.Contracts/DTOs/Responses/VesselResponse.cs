namespace Marilog.Contracts.DTOs.Responses
{
    public class VesselResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? IMONumber { get; set; } = null!;
        public decimal? GrossTonnage { get; set; }
        public int CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public int? FlagCountryId { get; set; }
        public string? FlagCountryName { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
    }
}
