using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Responses
{
    public class CargoTypeResponse
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public CargoCategory Category { get; set; }
        public CargoUnit DefaultUnit { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }
}