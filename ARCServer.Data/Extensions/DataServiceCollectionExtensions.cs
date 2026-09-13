using ARCServer.Data.Context;
using ARCServer.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ARCServer.Data.Extensions
{
    public static class DataServiceCollectionExtensions
    {
        public static IServiceCollection AddArcData(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<ArcDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("ApplicationDb")));

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
