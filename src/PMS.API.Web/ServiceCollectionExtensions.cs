namespace PMS.API.Web;
public static class ServiceCollectionExtensions
{
  public static void InstallPMSApplications(this IServiceCollection services, IConfiguration Configuration)
  {

    var assembly = AppDomain.CurrentDomain.Load("PMS.API.Application");
    services.AddMediatR(_ => _.RegisterServicesFromAssembly(assembly));

    services.AddAutoMapper(assembly);
  }
}
