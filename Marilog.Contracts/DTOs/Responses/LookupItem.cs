
namespace Marilog.Contracts.DTOs.Responses
{
    public class LookupItem<T>
    {
        public LookupItem(T id, string name)
        {
            Id = id;
            Name = name;
        }
        public T Id { get; set; } = default!;
        public string Name { get; set; } = string.Empty;
    }
}
