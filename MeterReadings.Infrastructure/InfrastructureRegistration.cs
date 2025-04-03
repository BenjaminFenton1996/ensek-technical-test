using MeterReadings.Infrastructure.Import;
using MeterReadings.Infrastructure.Import.Accounts;
using MeterReadings.Infrastructure.Import.MeterReadings;
using MeterReadings.Infrastructure.Initialization;
using MeterReadings.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MeterReadings.Infrastructure
{
    public static class InfrastructureRegistration
    {
        public static void RegisterInfrastructure(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<EnergyCompanyDbContext>(options => options.UseNpgsql(connectionString));
            services.AddScoped<IMeterReadingsRepository, MeterReadingsRepository>();
            services.AddScoped<IAccountsRepository, AccountsRepository>();
            services.AddScoped<ICsvHandler, AccountsCsvHandler>();
            services.AddScoped<ICsvHandler, MeterReadingsCsvHandler>();
            services.AddScoped<CsvImporter>();
            services.AddScoped<DatabaseBootstrapper>();
        }
    }
}
