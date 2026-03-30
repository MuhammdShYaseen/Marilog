using Marilog.Application.Interfaces.LogInterfaces;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Infrastructure.DataAccess.ContextDb;
using Marilog.Infrastructure.Repositories;
using Marilog.Infrastructure.Services;
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
            services.AddDbContext<MarilogContext>(options =>       // ← اسم الـ Context الفعلي
    options.UseSqlServer(
        configuration.GetConnectionString("DefaultConnection"),
        sql => sql
            .MigrationsAssembly("Marilog.Infrastructure")  // ← اسم المشروع الفعلي
            .EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null)));

            // ── Generic Repository — covers all Aggregate Roots ───────────────────
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<ILogReaderService, LogReaderService>();
            return services;
        }
    }
}
