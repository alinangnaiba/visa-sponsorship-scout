using Migrator.Application.Services;
using Raven.Client.Documents;

namespace Migrator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddSingleton<IDocumentStore>(provider =>
            {
                var store = new DocumentStore
                {
                    Urls = new[] { "http://localhost:8080" }, // Replace with your RavenDB server URL
                    Database = "Migrator"            // Database name
                };
                store.Initialize();
                return store;
            });
            builder.Services.AddScoped(provider =>
            {
                var store = provider.GetRequiredService<IDocumentStore>();
                return store.OpenAsyncSession(); // Scoped session for each request
            });

            builder.Services.AddScoped<IDataUploader, DataUploader>();
            builder.Services.AddScoped<IDataRetriever, DataRetriever>();

            builder.Services.AddRouting(options => options.LowercaseUrls = true);
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
