using Marilog.Infrastructure.DataAccess.ContextDb;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Infrastructure.Persistence
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMarilogPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<MarilogContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions => sqlOptions.UseNetTopologySuite()));

            return services;
        }
    }
}
