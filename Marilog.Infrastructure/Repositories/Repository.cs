using Marilog.Domain.Common;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Infrastructure.DataAccess.ContextDb;
using Microsoft.EntityFrameworkCore;

namespace Marilog.Infrastructure.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : Entity
    {
        private readonly MarilogContext _context;
        private readonly DbSet<TEntity> _set;

        public Repository(MarilogContext context)
        {
            _context = context;
            _set = context.Set<TEntity>();
        }

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<TEntity?> GetByIdAsync(int id, CancellationToken ct = default)
            => await _set.FindAsync(new object[] { id }, ct);

        /// <summary>
        /// Returns an IQueryable scoped to active records by default.
        /// Chain .IgnoreQueryFilters() if you need inactive records too.
        /// </summary>
        public IQueryable<TEntity> Query()
            => _set.AsQueryable();

        // ── Commands ──────────────────────────────────────────────────────────────

        public async Task AddAsync(TEntity entity, CancellationToken ct = default)
            => await _set.AddAsync(entity, ct);

        public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
            => await _set.AddRangeAsync(entities, ct);

        public void Update(TEntity entity)
            => _set.Update(entity);

        public void HardDelete(TEntity entity)
            => _set.Remove(entity);

        public void RemoveRange(IEnumerable<TEntity> entities)
            => _set.RemoveRange(entities);

        // ── Persistence ───────────────────────────────────────────────────────────

        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
            => await _context.SaveChangesAsync(ct);
    }
}