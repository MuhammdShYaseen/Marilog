using Marilog.Contracts.DTOs.Frontend;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.FrontendServices;
using Marilog.Domain.Entities.SystemEntities;
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

        public async Task<NavItemResponse> CreateAsync(string title, string? route, string? icon, int? parentId, int order, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title is required");

            var normalizedTitle = title.Trim().ToUpperInvariant();

            // Check duplicate
            var isExist = await _repo.Query()
                .AnyAsync(n => n.Title.ToUpper() == normalizedTitle, ct);

            if (isExist)
                throw new ArgumentException("Nav item already exists");

            // Validate parent
            if (parentId.HasValue)
            {
                var parentExists = await _repo.Query()
                    .AnyAsync(n => n.Id == parentId.Value, ct);

                if (!parentExists)
                    throw new ArgumentException("Parent not found");
            }

            var navItem = NavItem.Create(
                title.Trim(),
                route,
                icon,
                parentId,
                order);

            await _repo.AddAsync(navItem, ct);
            await _repo.SaveChangesAsync(ct);

            return new NavItemResponse
            {
                Title = navItem.Title,
                Route = navItem.Route,
                Order = navItem.Order,
                Icon = navItem.Icon
            };
        }

        public async Task<List<NavItemResponse>> CreateRangeAsync(List<CreateNavItemRequest> items, CancellationToken ct = default)
        {
            if (items == null || !items.Any())
                throw new ArgumentException("Items are required");

            // Normalize titles
            var normalizedTitles = items
                .Select(i => i.Title?.Trim().ToUpperInvariant())
                .ToList();

            // Check duplicates inside request
            var duplicateInRequest = normalizedTitles
                .GroupBy(t => t)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateInRequest.Any())
                throw new ArgumentException("Duplicate titles in request");

            // Check duplicates in database
            var existingTitles = await _repo.Query()
                .Where(n => normalizedTitles.Contains(n.Title.ToUpper()))
                .Select(n => n.Title.ToUpper())
                .ToListAsync(ct);

            if (existingTitles.Any())
                throw new ArgumentException("Some nav items already exist");

            // Validate parents
            var parentIds = items
                .Where(i => i.ParentId.HasValue)
                .Select(i => i.ParentId!.Value)
                .Distinct()
                .ToList();

            if (parentIds.Any())
            {
                var existingParents = await _repo.Query()
                    .Where(n => parentIds.Contains(n.Id))
                    .Select(n => n.Id)
                    .ToListAsync(ct);

                var missingParents = parentIds
                    .Except(existingParents)
                    .ToList();

                if (missingParents.Any())
                    throw new ArgumentException("Some parent items not found");
            }

            // Create entities
            var navItems = items
                .Select(i => NavItem.Create(
                    i.Title.Trim(),
                    i.Route,
                    i.Icon,
                    i.ParentId,
                    i.Order))
                .ToList();

            await _repo.AddRangeAsync(navItems, ct);
            await _repo.SaveChangesAsync(ct);

            return navItems
                .Select(n => new NavItemResponse
                {
                    Title = n.Title,
                    Route = n.Route,
                    Order = n.Order,
                    Icon = n.Icon
                })
                .ToList();
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


        public async Task<NavItemResponse> UpdateAsync(int id, string title, string? route, string? icon, int? parentId, int order, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title is required");

            var navItem = await _repo.GetByIdAsync(id, ct);

            if (navItem == null)
                throw new ArgumentException("Nav item not found");

            var normalizedTitle = title.Trim().ToUpperInvariant();

            // Check duplicate (exclude current)
            var isDuplicate = await _repo.Query()
                .AnyAsync(n =>
                    n.Id != id &&
                    n.Title.ToUpper() == normalizedTitle,
                    ct);

            if (isDuplicate)
                throw new ArgumentException("Nav item already exists");

            // Validate parent
            if (parentId.HasValue)
            {
                if (parentId.Value == id)
                    throw new ArgumentException("Item cannot be its own parent");

                var parentExists = await _repo.Query()
                    .AnyAsync(n => n.Id == parentId.Value, ct);

                if (!parentExists)
                    throw new ArgumentException("Parent not found");
            }

            // Update entity
            navItem.Update(
                title.Trim(),
                route,
                icon,
                parentId,
                order);

            await _repo.SaveChangesAsync(ct);

            return new NavItemResponse
            {
                Title = navItem.Title,
                Route = navItem.Route,
                Order = navItem.Order,
                Icon = navItem.Icon
            };
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var navItem = await _repo.GetByIdAsync(id, ct);

            if (navItem == null)
                throw new ArgumentException("Nav item not found");

            // Check children
            var hasChildren = await _repo.Query()
                .AnyAsync(n => n.ParentId == id, ct);

            if (hasChildren)
                throw new ArgumentException(
                    "Cannot delete item with children");

            _repo.HardDelete(navItem);

            await _repo.SaveChangesAsync(ct);
        }
    }
}
