
namespace Marilog.Contracts.DTOs.Responses
{
    public class LookupItem<T>
    {
        public T Id { get; set; } = default!;
        public string Name { get; set; } = string.Empty;
    }
}
