
using Marilog.Intelligence.Worker.Services;
using Qdrant.Client;

namespace Marilog.Intelligence.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            builder.Services.AddHttpClient<AiProviderService>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["Urls:Presentation"]
                    ?? throw new InvalidOperationException("MarіlogApi:Presentation not configured"));
            });

            builder.Services.AddHttpClient(); // IHttpClientFactory للـ AI calls

            // Qdrant
            builder.Services.AddSingleton(new QdrantClient(
                builder.Configuration["Qdrant:Host"] ?? "localhost",
                int.Parse(builder.Configuration["Qdrant:Port"] ?? "6334")));

            // Services
            builder.Services.AddScoped<EmbeddingService>();
            builder.Services.AddScoped<VectorStoreService>();
            builder.Services.AddScoped<SemanticSearchService>();
            builder.Services.AddScoped<RagService>();

            builder.Services.AddEndpointsApiExplorer();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
