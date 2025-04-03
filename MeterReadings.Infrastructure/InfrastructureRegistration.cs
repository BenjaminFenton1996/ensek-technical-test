using MeterReadings.Infrastructure.Initialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MeterReadings.Infrastructure
{
    public static class InfrastructureRegistration
    {
        public static void RegisterInfrastructure(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<EnergyCompanyDbContext>(options => options.UseNpgsql(connectionString));
            services.AddScoped<DatabaseBootstrapper>();
        }
    }
}
