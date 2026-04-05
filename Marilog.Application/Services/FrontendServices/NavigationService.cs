using Marilog.Application.DTOs.Responses;
using Marilog.Application.Interfaces.FrontendServices;
using Marilog.Domain.Entities;
using Marilog.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;


namespace Marilog.Application.Services.FrontendServices
{
    public class NavigationService : INavigationService
    {
        private readonly IRepository<NavItem> _repo;

        public NavigationService(IRepository<NavItem> repo)
        {
            _repo = repo;
        }

        public async Task<List<NavItemResponse>> GetAsync(CancellationToken ct = default)
        {
            var items = await _repo.Query()
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Order)
                .Select(x => new
                {
                    x.Id,
                    x.Title,
                    x.Route,
                    x.Icon,
                    x.ParentId,
                    x.Order
                })
                .ToListAsync(ct);

            var map = items.ToDictionary(
                x => x.Id,
                x => new NavItemResponse
                {
                    Title = x.Title,
                    Route = x.Route,
                    Icon = x.Icon,
                    Order = x.Order
                });

            var root = new List<NavItemResponse>();

            foreach (var item in items)
            {
                var current = map[item.Id];

                if (item.ParentId is null)
                    root.Add(current);
                else if (map.TryGetValue(item.ParentId.Value, out var parent))
                    parent.Children.Add(current);
            }

            return root.OrderBy(x => x.Order).ToList();
        }
    }
}
