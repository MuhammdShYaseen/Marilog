using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Requests.CargoTypeDTOs
{
    public class CreateCargoTypeRequest
    {
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public CargoCategory Category { get; set; }
        public CargoUnit DefaultUnit { get; set; }
        public int SortOrder { get; set; }
    }
}