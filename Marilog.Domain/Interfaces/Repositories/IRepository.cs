using Marilog.Domain.Common;

namespace Marilog.Domain.Interfaces.Repositories
{
    /// </summary>
    /// <summary>
    /// Generic repository contract for all Aggregate Roots.
    /// Complex queries are built via Query() using LINQ directly in the service layer.
    /// </summary>
    public interface IRepository<TEntity> where TEntity : Entity
    {
        // ── Queries ───────────────────────────────────────────────────────────────
        Task<TEntity?> GetByIdAsync(int id, CancellationToken ct = default);

        /// <summary>
        /// Entry point for all custom queries.
        /// Use with .Where() .Include() .Select() etc. in the service layer.
        /// </summary>
        IQueryable<TEntity> Query();

        // ── Commands ──────────────────────────────────────────────────────────────
        Task AddAsync(TEntity entity, CancellationToken ct = default);
        Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default);
        void Update(TEntity entity);
        void HardDelete(TEntity entity);
        void RemoveRange(IEnumerable<TEntity> entities);

        // ── Persistence ───────────────────────────────────────────────────────────
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
