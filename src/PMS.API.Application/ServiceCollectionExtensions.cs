using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PMS.API.Application.Common.BackgroundWorker;
using PMS.API.Application.Common.Helpers;
using PMS.API.Application.Identity;
using PMS.API.Application.Services.Implementation;
using PMS.API.Application.Services.Interfaces;
using PMS.API.Core.Domain.Entities.Identity;
using PMS.API.Infrastructure.Data;
using IdentityConstants = PMS.API.Application.Identity.IdentityConstants;

namespace PMS.API.Application;

public static class ServiceCollectionExtensions
{
  /// <summary>
  ///     Adds Security Authentication.
  /// </summary>


  public static void InstallPMSServices(this IServiceCollection services, IConfiguration configuration)
  {
    RegisterData(services);
    RegisterServices(services);
    RegisterAuth(services, configuration);
  }


  private static void RegisterServices(IServiceCollection services)
  {
    services.AddTransient<IIdentityService, IdentityService>();
    services.AddTransient<IRoleService, RoleService>();
    services.AddTransient<IClaimApplication, ClaimApplication>();
    services.AddTransient<IExceptionHelper, ExceptionHelper>();
    services.AddTransient<IEmailSenderService, EmailSenderService>();
    services.AddScoped<IDocumentService, DocumentService>();
    services.AddScoped<IPdfService, PdfService>();
    services.AddScoped<IInterFaxService, InterFaxService>();
    services.AddScoped<IOrderFaxService, OrderFaxService>();
    services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
    services.AddHostedService<QueuedHostedService>();
  }

  private static void RegisterData(IServiceCollection services)
  {
    services.AddIdentity<User, Role>(options =>
    {
      options.Password.RequireLowercase = true;
      options.Password.RequireUppercase = true;
      options.Password.RequiredLength = 8;
      options.Password.RequireNonAlphanumeric = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddUserStore<ExtendedUserStore>()
    .AddRoleValidator<TenantRoleValidator>()
    .AddDefaultTokenProviders()
    .AddTokenProvider(IdentityConstants.RefreshTokenProvider, typeof(DataProtectorTokenProvider<User>));
    var defaultRoleValidator = services.FirstOrDefault(descriptor => descriptor.ImplementationType == typeof(RoleValidator<Role>));
    if (defaultRoleValidator != null) { services.Remove(defaultRoleValidator); }
  }

  private static void RegisterAuth(IServiceCollection services, IConfiguration configuration)
  {
    services.Configure<JwtOptions>(configuration.GetSection(nameof(JwtOptions)));
    var builderService = services.BuildServiceProvider();
    var claimApplication = builderService.GetRequiredService<IClaimApplication>();
    services.RegisterPolicy(claimApplication);

    var jwtOptions = new JwtOptions();
    configuration.Bind(nameof(JwtOptions), jwtOptions);

    services.AddAuthentication(options =>
    {
      options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
      options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
      options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
        .AddJwtBearer(options =>
        {
          options.RequireHttpsMetadata = false;
          options.Events = new JwtBearerEvents();

          options.TokenValidationParameters = new TokenValidationParameters
          {
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = JwtTokenBuilder.CreateSigningKey(jwtOptions.Secret!),
            ClockSkew = TimeSpan.Zero,
          };
        });
  }
}
