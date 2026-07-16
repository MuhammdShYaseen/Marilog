using Marilog.Application.Interfaces.DataManagment;
using Marilog.Infrastructure.DataAccess.ContextDb;
using Microsoft.EntityFrameworkCore;

namespace Marilog.Infrastructure.Services
{
    public class EfCoreSchemaVersionProvider : ISchemaVersionProvider
    {
        private readonly MarilogContext _context;

        public EfCoreSchemaVersionProvider(MarilogContext context)
        {
            _context = context;
        }

        public async Task<string> GetCurrentVersionAsync(CancellationToken ct = default)
        {
            var applied = await _context.Database.GetAppliedMigrationsAsync(ct);
            return applied.LastOrDefault() ?? "no-migrations-applied";
        }
    }
}
