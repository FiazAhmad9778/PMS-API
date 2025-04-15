using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PMS.API.Infrastructure.Data;

namespace PMS.API.Infrastructure;

public static class StartupSetup
{
  public static void AddDbContext(this IServiceCollection services, string connectionString) =>
       services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString), ServiceLifetime.Scoped);

}
