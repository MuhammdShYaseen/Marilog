
using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Requests.CargoTypeDTOs
{
    /// <summary>Code and Category are fixed after creation.</summary>
    public class UpdateCargoTypeRequest
    {
        public string Name { get; set; } = default!;
        public CargoUnit DefaultUnit { get; set; }
        public int SortOrder { get; set; }
    }
}