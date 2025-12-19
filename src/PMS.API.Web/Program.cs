using Amazon.S3;
using Ardalis.ListStartupServices;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Hangfire;
using Hangfire.PostgreSql;
using HealthChecks.UI.Client;
using LiteDB;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi.Models;
using PMS.API.Application;
using PMS.API.Application.Common.Helpers;
using PMS.API.Application.Services.Interfaces;
using PMS.API.Core;
using PMS.API.Infrastructure;
using PMS.API.Infrastructure.Data;
using PMS.API.Infrastructure.Interfaces;
using PMS.API.Web;
using PMS.API.Web.Common;
using PMS.API.Web.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

builder.Services.Configure<CookiePolicyOptions>(options =>
{
  options.CheckConsentNeeded = context => true;
  options.MinimumSameSitePolicy = SameSiteMode.None;
});


string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext(connectionString!);

#region context services
builder.Services.AddScoped<ICurrentUserService, DefaultCurrentUserService>();
#endregion

builder.Services.AddSingleton<IAmazonS3>(provider => new AmazonS3Client(builder.Configuration.GetValue<string>("AWS:AccessKey"), builder.Configuration.GetValue<string>("AWS:SecretAccessKey"), Amazon.RegionEndpoint.EUNorth1));
builder.Services.InstallPMSApplications(builder.Configuration);
builder.Services.InstallRepositories();
builder.Services.InstallPMSServices(builder.Configuration);
builder.Services.AddHttpClient();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddControllers(options =>
{
  options.Conventions.Add(new LowercaseControllerNameTransformConvention());
}).AddNewtonsoftJson();

builder.Services.AddTransient<IExceptionHelper, ExceptionHelper>();

//swagger
builder.Services.AddSwaggerGen(c =>
{
  c.SwaggerDoc("v1", new OpenApiInfo { Title = "PMS API", Version = "v1" });
  c.EnableAnnotations();
  c.DescribeAllParametersInCamelCase();
  // Configure Swagger to use bearer token authentication
  c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
  {
    Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
    Name = "Authorization",
    In = ParameterLocation.Header,
    Type = SecuritySchemeType.ApiKey,
    Scheme = "Bearer"
  });
  c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
    }
  });
});

//CORS policy
builder.Services.AddCors(options =>
{
  options.AddPolicy("MyCorsPolicy", builder =>
  {
    builder.AllowAnyOrigin()
           .AllowAnyHeader()
           .AllowAnyMethod();
  });
});

builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
  containerBuilder.RegisterModule(new DefaultCoreModule());
  containerBuilder.RegisterModule(new DefaultInfrastructureModule(builder.Environment.EnvironmentName == "Development"));
});
builder.Services.AddHealthChecks().AddNpgSql(connectionString!);
//await builder.Services.SeedUserAsync();

#region Application Insights
builder.Services.AddApplicationInsightsTelemetry();
#endregion


// ✅ Configure Hangfire with PostgreSQL using the new method
builder.Services.AddHangfire(config =>
    config.UsePostgreSqlStorage(options =>
    {
      options.UseNpgsqlConnection(connectionString);
    },
    new PostgreSqlStorageOptions
    {
      SchemaName = "hangfire"
    }));

builder.Services.AddHangfireServer(options => options.WorkerCount = 1); // ✅ Ensures only 1 job runs at a time



var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
  ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

if (app.Environment.IsDevelopment())
{
  app.UseDeveloperExceptionPage();
  app.UseShowAllServicesMiddleware();
}
else
{
  app.UseHsts();
}
app.UseCors("MyCorsPolicy"); // Apply the CORS policy to all APIs
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCookiePolicy();


// Enable middle ware to serve generated Swagger as a JSON endpoint.
app.UseSwagger();

// Enable middle ware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PMS V1"));

app.MapDefaultControllerRoute();

app.MapHealthChecks("/health",
    new HealthCheckOptions
    {
      ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

// Seed Database
using (var scope = app.Services.CreateScope())
{
  var services = scope.ServiceProvider;

  try
  {
    var context = services.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();

    // ✅ Ensure Hangfire storage is initialized before scheduling jobs
    var recurringJobManager = services.GetRequiredService<IRecurringJobManager>();

    recurringJobManager.AddOrUpdate<IDocumentService>(
        "sync-documents",
        service => service.SyncDocuments(CancellationToken.None),
        "*/2 * * * *"
    );

    // Schedule recurring job to process pending orders every 10 minutes
    recurringJobManager.AddOrUpdate<IOrderFaxService>(
        "process-pending-orders",
        service => service.ProcessPendingOrdersAsync(CancellationToken.None),
        "*/2 * * * *" // Every 10 minutes
    );

  }
  catch (Exception ex)
  {
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred seeding the DB. {exceptionMessage}", ex.Message);
  }
}
app.UseHangfireDashboard();
app.Run();

// Make the implicit Program.cs class public, so integration tests can reference the correct assembly for host building
public partial class Program
{
  protected Program() { }
}
