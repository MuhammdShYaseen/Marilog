
namespace Marilog.Contracts.Common
{
    public sealed class PagedRequest
    {
        private const int MaxPageSize = 100;

        private int _page = 1;
        private int _pageSize = 20;

        public int Page
        {
            get => _page;
            init => _page = value <= 0 ? 1 : value;
        }

        public int PageSize
        {
            get => _pageSize;
            init => _pageSize = value <= 0
                ? 20
                : Math.Min(value, MaxPageSize);
        }
    }
}
