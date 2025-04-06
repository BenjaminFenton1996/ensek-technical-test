
using MeterReadings.API.Services;
using MeterReadings.Infrastructure;
using MeterReadings.Infrastructure.Initialization;
using Microsoft.EntityFrameworkCore;

namespace MeterReadings.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //Configure DB Context
            var connectionString = builder.Configuration.GetConnectionString("EnergyCompanyDb");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Database connection string was not found or was empty in configuration");
            }
            builder.Services.RegisterInfrastructure(connectionString);
            builder.Services.AddScoped<IUploadMeterReadingsService, UploadMeterReadingsService>();

            var app = builder.Build();
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var bootstrapper = services.GetRequiredService<DatabaseBootstrapper>();
                await bootstrapper.RunBootstrapper(builder.Environment.IsDevelopment());
            }

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
