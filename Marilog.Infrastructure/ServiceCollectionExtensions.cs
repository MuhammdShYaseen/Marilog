using Marilog.Domain.Interfaces.Repositories;
using Marilog.Infrastructure.DataAccess.ContextDb;
using Marilog.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // ── DbContext ─────────────────────────────────────────────────────────
            services.AddDbContext<MarilogContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sql => sql
                        .MigrationsAssembly(typeof(MarilogContext).Assembly.FullName)
                        .EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null)));

            // ── Generic Repository — covers all Aggregate Roots ───────────────────
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            return services;
        }
    }
}
