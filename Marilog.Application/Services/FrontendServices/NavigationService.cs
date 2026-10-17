using Marilog.Contracts.DTOs.Frontend.NavItem;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.FrontendServices;
using Marilog.Domain.Entities.Frontend;
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
                .Select(ToResponse)
                .ToList();
        }


        public Task<List<NavItemResponse>> GetAsync(CancellationToken ct = default)
     => GetTreeAsync(activeOnly: true, ct);

        public Task<List<NavItemResponse>> GetAllAsync(CancellationToken ct = default)
            => GetTreeAsync(activeOnly: false, ct);

        private async Task<List<NavItemResponse>> GetTreeAsync(bool activeOnly, CancellationToken ct)
        {
            var query = _repo.Query().AsNoTracking();

            if (activeOnly)
                query = query.Where(x => x.IsActive);

            var items = await query
                .OrderBy(x => x.Order)
                .Select(x => new NavItemResponse
                {
                    Id = x.Id,
                    ParentId = x.ParentId,
                    Title = x.Title,
                    Route = x.Route,
                    Icon = x.Icon,
                    Order = x.Order,
                    IsActive = x.IsActive
                })
                .ToListAsync(ct);

            var map = items.ToDictionary(x => x.Id);
            var root = new List<NavItemResponse>();

            foreach (var item in items)
            {
                if (item.ParentId is null)
                    root.Add(item);
                else if (map.TryGetValue(item.ParentId.Value, out var parent))
                    parent.Children.Add(item);
            }

            return root;
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





        public async Task SetActiveAsync(int id, bool isActive, CancellationToken ct = default)
        {
            var navItem = await _repo.GetByIdAsync(id, ct)
                ?? throw new ArgumentException("Nav item not found");

            navItem.SetActive(isActive);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task SaveLayoutAsync(List<NavItemLayoutRequest> items, CancellationToken ct = default)
        {
            if (items == null || items.Count == 0)
                throw new ArgumentException("Layout is empty");

            if (items.GroupBy(i => i.Id).Any(g => g.Count() > 1))
                throw new ArgumentException("Duplicate items in layout");

            var all = await _repo.Query().ToListAsync(ct);
            var byId = all.ToDictionary(n => n.Id);

            // Resulting parent of every item = current parent, overridden by the request
            var parentOf = all.ToDictionary(n => n.Id, n => n.ParentId);

            foreach (var i in items)
            {
                if (!byId.ContainsKey(i.Id))
                    throw new ArgumentException($"Nav item {i.Id} not found");

                if (i.ParentId == i.Id)
                    throw new ArgumentException("Item cannot be its own parent");

                if (i.ParentId.HasValue && !byId.ContainsKey(i.ParentId.Value))
                    throw new ArgumentException($"Parent {i.ParentId} not found");

                parentOf[i.Id] = i.ParentId;
            }

            // Circular reference check (A under B, B under A)
            foreach (var id in parentOf.Keys)
            {
                var seen = new HashSet<int> { id };
                for (var p = parentOf[id]; p.HasValue; p = parentOf[p.Value])
                {
                    if (!seen.Add(p.Value))
                        throw new ArgumentException("Layout contains a circular parent reference");
                }
            }

            foreach (var i in items)
                byId[i.Id].Move(i.ParentId, i.Order);

            await _repo.SaveChangesAsync(ct);
        }

        public async Task<List<NavItemExport>> ExportAsync(CancellationToken ct = default)
        {
            var tree = await GetAllAsync(ct);
            return tree.Select(ToExport).ToList();
        }

        public async Task ImportAsync(List<NavItemExport> items, CancellationToken ct = default)
        {
            if (items == null || items.Count == 0)
                throw new ArgumentException("Import file is empty");

            var flat = Flatten(items).ToList();

            foreach (var x in flat)
            {
                if (string.IsNullOrWhiteSpace(x.Title))
                    throw new ArgumentException("Every nav item needs a title");

                if (x.Title.Trim().Length > 100 || x.Route?.Length > 200 || x.Icon?.Length > 50)
                    throw new ArgumentException($"Field too long in '{x.Title}'");
            }

            var duplicate = flat
                .GroupBy(x => x.Title.Trim().ToUpperInvariant())
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicate != null)
                throw new ArgumentException($"Duplicate title in import: {duplicate.Key}");

            // Replace the whole menu in one SaveChanges
            var existing = await _repo.Query().ToListAsync(ct);
            foreach (var n in existing)
                _repo.HardDelete(n);

            var roots = items.Select((x, i) => BuildTree(x, i)).ToList();
            await _repo.AddRangeAsync(roots, ct);
            await _repo.SaveChangesAsync(ct);
        }

        // ── Import/Export helpers ──────────────────────────────────────────────

        private static NavItemExport ToExport(NavItemResponse x) => new()
        {
            Title = x.Title,
            Route = x.Route,
            Icon = x.Icon,
            IsActive = x.IsActive,
            Children = x.Children.Select(ToExport).ToList()
        };

        private static IEnumerable<NavItemExport> Flatten(IEnumerable<NavItemExport> items)
            => items.SelectMany(x => Flatten(x.Children ?? new()).Prepend(x));

        private static NavItem BuildTree(NavItemExport source, int order)
        {
            var item = NavItem.Create(source.Title.Trim(), Blank(source.Route), Blank(source.Icon), null, order);

            if (!source.IsActive)
                item.SetActive(false);

            var children = source.Children ?? new();
            for (var i = 0; i < children.Count; i++)
                item.AddChild(BuildTree(children[i], i));

            return item;
        }

        private static string? Blank(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();


        private static NavItemResponse ToResponse(NavItem n) => new()
        {
            Id = n.Id,
            ParentId = n.ParentId,
            Title = n.Title,
            Route = n.Route,
            Icon = n.Icon,
            Order = n.Order,
            IsActive = n.IsActive
        };
    }
}
