namespace Marilog.Contracts.Common
{
    public sealed class PagedResponse<T>
    {
        private readonly int? _totalPages;
        public List<T> Items { get; init; } = [];
        public int TotalCount { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int TotalPages
        {
            get => _totalPages ?? (int)Math.Ceiling((double)TotalCount / PageSize);
            init => _totalPages = value;
        }
        public bool HasNext => Page < TotalPages;
        public bool HasPrev => Page > 1;

    }
}
