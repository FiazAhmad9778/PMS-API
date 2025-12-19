using Microsoft.Extensions.DependencyInjection;
using PMS.API.Core.Domain.Interfaces;
using PMS.API.Core.Domain.Interfaces.Repositories;
using PMS.API.Infrastructure.Repositories;
namespace PMS.API.Infrastructure;

public static class RepositoryInstaller
{
  public static void InstallRepositories(this IServiceCollection services)
  {
    services.AddTransient(typeof(IRepository<>), typeof(BaseRepository<>));
    services.AddTransient<IUserRepository, UserRepository>();
    services.AddTransient<IDocumentRepository, DocumentRepository>();
    services.AddTransient<IOrderRepository, OrderRepository>();
  }
}
