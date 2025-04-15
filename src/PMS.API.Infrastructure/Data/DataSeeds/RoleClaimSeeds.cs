using Microsoft.EntityFrameworkCore;
using PMS.API.Core.Domain.Entities.Identity;
namespace PMS.API.Infrastructure.Data.DataSeeder;
public static class RoleClaimSeeds
{
  public static void SeedRoleClaims(ModelBuilder modelBuilder)
  {
    var rolesToSeed = new List<Role>
      {
            new Role { Id =1L, Name = "Superadmin", NormalizedName="SUPERADMIN", DisplayName = "Super Admin", IsSystem = true },
            new Role { Id =2L, Name = "Technician", NormalizedName="TECHNICIAN", DisplayName = "Technician", IsSystem = false },
        };

    modelBuilder.Entity<Role>().HasData(rolesToSeed);


    var roleClaims = new List<RoleClaim>
      {
            new RoleClaim { Id=1, RoleId = 1, ApplicationClaimId = 1, IsAssigned = true },
            new RoleClaim { Id=2, RoleId = 1, ApplicationClaimId = 2, IsAssigned = true },
            new RoleClaim { Id=3, RoleId = 1, ApplicationClaimId = 3, IsAssigned = true },
            new RoleClaim { Id=4, RoleId = 1, ApplicationClaimId = 4, IsAssigned = true },
            new RoleClaim { Id=5, RoleId = 2, ApplicationClaimId = 5, IsAssigned = true },
            new RoleClaim { Id=6, RoleId = 2, ApplicationClaimId = 6, IsAssigned = true },
            new RoleClaim { Id=7, RoleId = 2, ApplicationClaimId = 7, IsAssigned = true },
            new RoleClaim { Id=8, RoleId = 2, ApplicationClaimId = 8, IsAssigned = true },
        };
    modelBuilder.Entity<RoleClaim>().HasData(roleClaims);
  }
}
