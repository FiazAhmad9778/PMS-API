// AppDbContextFactory.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PMS.API.Infrastructure.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
  public AppDbContext CreateDbContext(string[] args)
  {
    var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

    // Use SqlServer at design time so the built model matches the snapshot (SqlServer).
    // This avoids NullReferenceException in MigrationsModelDiffer.TryGetDefaultValue when
    // comparing snapshot (SqlServer) vs model (Npgsql). Runtime still uses Npgsql via Program/Startup.
    optionsBuilder.UseSqlServer("Server=.;Database=PMS;Trusted_Connection=True;TrustServerCertificate=True;");

    return new AppDbContext(optionsBuilder.Options);
  }
}
