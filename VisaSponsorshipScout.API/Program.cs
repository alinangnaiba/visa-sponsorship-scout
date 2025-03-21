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
                            IEnumerable<string> allowedCorsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").GetChildren().Select(x => x.Value);
                            policy.WithOrigins(allowedCorsOrigins.ToArray());
                        }
                        else
                        {
                            policy.SetIsOriginAllowed(_ => true);
                        }
                    });
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
            app.UseCors();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
