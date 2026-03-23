using Marilog.Application;
using Marilog.Infrastructure;
using Marilog.Presentation.Extensions;
using Serilog;
namespace Marilog.Presentation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.ConfigureSerilog(); 
            builder.Services
                .AddInfrastructure(builder.Configuration)
                .AddApplication()
                .AddApiServices();

            builder.Services.AddControllers();
            builder.Services.AddOpenApi(options =>
            {
                // Specify the OpenAPI version to use
                options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_1;
            });

            var app = builder.Build();
            app.UseSerilogRequestLogging(opts =>
            {
                opts.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.000}ms";
            });
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseErrorHandler();
            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
