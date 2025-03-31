using FastEndpoints;
using FastEndpoints.Swagger;
using VisaSponsorshipScout.Application.Services;
using VisaSponsorshipScout.Infrastructure.Extensions;

namespace VisaSponsorshipScout.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.ConfigureDatabase(builder.Configuration);

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    policy =>
                    {
                        policy
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                        if (bool.TryParse(builder.Configuration.GetSection("Cors:Enabled").Value, out bool isEnabled) && isEnabled)
                        {
                            IEnumerable<string?> allowedCorsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").GetChildren().Select(x => x.Value);
                            policy.WithOrigins(allowedCorsOrigins.Where(origin => origin != null).ToArray()!);
                        }
                        else
                        {
                            policy.SetIsOriginAllowed(_ => true);
                        }
                    });
            });

            builder.Services.AddScoped<IDataRetriever, DataRetriever>();

            builder.Services.AddRouting(options => options.LowercaseUrls = true);
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddFastEndpoints();
            if (builder.Environment.IsDevelopment())
            {
                builder.Services.SwaggerDocument(doc =>
                {
                    doc.DocumentSettings = setting =>
                    {
                        setting.Title = "Visa Sponsorship Scout API";
                        setting.Description = "API for Visa Sponsorship Scout";
                        setting.Version = "v1";
                    };
                });
            }

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseHttpsRedirection();
            app.UseCors();

            app.UseFastEndpoints(cfg =>
            {
                cfg.Endpoints.RoutePrefix = "api";
                cfg.Endpoints.Configurator = ep => ep.Routes.Select(route => route.ToLower());
            })
                .UseSwaggerGen();

            if (app.Environment.IsProduction())
            {
                string port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
                string url = $"http://0.0.0.0:{port}";
                app.Run(url);
            }
            else
            {
                app.Run();
            }
        }
    }
}