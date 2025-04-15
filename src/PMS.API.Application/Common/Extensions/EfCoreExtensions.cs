using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace PMS.API.Application.Common.Extensions;
public static class EfCoreExtensions
{
  public static async Task<bool> HasPendingMigrations(this DbContext dbContext)
  {
    var migrationsAssembly = dbContext.GetService<IMigrationsAssembly>();
    var historyRepository = dbContext.GetService<IHistoryRepository>();
    var all = migrationsAssembly.Migrations.Keys;
    var applied = (await historyRepository.GetAppliedMigrationsAsync()).Select(r => r.MigrationId);
    return all.Except(applied).Any();
  }
}
