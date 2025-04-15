using Microsoft.EntityFrameworkCore;
using PMS.API.Core.Domain.Entities.Identity;
using PMS.API.Infrastructure.Data.DataSeeder;

namespace PMS.API.Infrastructure.Data.DataSeeds;
public static class DataSeeder
{
  public static void SeedData(ModelBuilder modelBuilder)
  {
    #region ClaimGroup

    var claimGroups = new List<ClaimGroup>
    {
        new ClaimGroup { Id = 1,  Sequence=20, Name = "User Management", CreatedBy = 1, ModifiedBy = 1, IsDisplay = false, IsDeleted = false },
        new ClaimGroup { Id = 2,  Sequence=40, Name = "Reports Management", CreatedBy = 1, ModifiedBy = 1, IsDisplay = false, IsDeleted = false },
    };

    modelBuilder.Entity<ClaimGroup>().HasData(claimGroups);

    #endregion

    #region claims

    var userManagementClaims = new List<ApplicationClaim>
    {
        new ApplicationClaim { Id = 1, ClaimValue = "User Management Add", ClaimCode = "CGMA", ClaimGroupId = 1 , IsAllowedToAll = false},
        new ApplicationClaim { Id = 2, ClaimValue = "User Management Edit", ClaimCode = "CGME", ClaimGroupId = 1, IsAllowedToAll = false },
        new ApplicationClaim { Id = 3, ClaimValue = "User Management View", ClaimCode = "CGMV", ClaimGroupId = 1, IsAllowedToAll = false },
        new ApplicationClaim { Id = 4, ClaimValue = "User Management Delete", ClaimCode = "CGMD", ClaimGroupId = 1, IsAllowedToAll = false }
    };

    var reportManagementClaims = new List<ApplicationClaim>
    {
        new ApplicationClaim { Id = 5, ClaimValue = "Report Management View", ClaimCode = "RMV", ClaimGroupId = 2 , IsAllowedToAll = true},
        new ApplicationClaim { Id = 6, ClaimValue = "Report Management Signature", ClaimCode = "RMS", ClaimGroupId = 2, IsAllowedToAll = true },
        new ApplicationClaim { Id = 7, ClaimValue = "Report Management Download", ClaimCode = "RMD", ClaimGroupId = 2, IsAllowedToAll = true },
        new ApplicationClaim { Id = 8, ClaimValue = "Report Management Print", ClaimCode = "RMP", ClaimGroupId = 2, IsAllowedToAll = true }
    };

    var allLists = new List<List<ApplicationClaim>>
    {
        userManagementClaims,
        reportManagementClaims
    };

    var applicationClaims = new List<ApplicationClaim>();

    foreach (var claimList in allLists)
    {
      applicationClaims.AddRange(claimList);
    }

    modelBuilder.Entity<ApplicationClaim>().HasData(applicationClaims);

    #endregion

    RoleClaimSeeds.SeedRoleClaims(modelBuilder);
  }
}
