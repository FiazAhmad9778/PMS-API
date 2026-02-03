using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PMS.API.Infrastructure.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
  public AppDbContext CreateDbContext(string[] args)
  {
    var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

    // Replace with your PostgreSQL connection string - must match the password that works in pgAdmin
    // Update the password below to match your actual PostgreSQL password
    optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=PMS;User Id=postgres;Password=Sorry2S@Y;SslMode=Prefer;TrustServerCertificate=true;");

    // Include any switches you need, e.g., Npgsql legacy timestamp behavior
    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

    return new AppDbContext(optionsBuilder.Options);
  }
}
