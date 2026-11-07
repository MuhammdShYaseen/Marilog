
namespace Marilog.Presentation.Extensions
{
    public static class ApiServiceCollectionExtensions
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new(){Title = "Marilog API", Version = "v1" });

                c.CustomSchemaIds(type => type.FullName);
            });

            services.AddCors(opt => opt.AddPolicy("AllowAll", p =>
                p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

            return services;
        }
    }
}
