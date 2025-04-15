using Microsoft.AspNetCore.Identity;
using PMS.API.Core.Domain.Entities.Identity;
using PMS.API.Infrastructure.Data;

namespace PMS.API.Web.Helpers;

public static class InitializeData
{
  public static async Task SeedUserAsync(this IServiceCollection services /*, string username, string email, string password*/)
  {

    var builder = services.BuildServiceProvider();
    var userManager = builder.GetRequiredService<UserManager<User>>();
    var roleManager = builder.GetRequiredService<RoleManager<Role>>();
    var _context = builder.GetRequiredService<AppDbContext>();

    // Check if the user already exists
    var existingUser = await userManager.FindByNameAsync("superadmin");
    if (existingUser == null)
    {
      // User does not exist, create a new user object
      var newUser = new User
      {
        UserName = "superadmin",
        Email = "superadmin@westmountpharmacy.co",
        FirstName = "Super",
        LastName = "Admin"
      };

      // Create the user with the password
      var result = await userManager.CreateAsync(newUser, "Asdf@123");

      // Check if the user creation was successful
      if (!result.Succeeded)
      {
        // Handle user creation failure, if any
        throw new Exception($"User creation failed: {string.Join(", ", result.Errors)}");
      }
      else
      {

        var role = _context.Roles.FirstOrDefault(x => x.Name == "Superadmin");

        if (role == null)
        {
          // "Super User" role doesn't exist, you may need to create it
          // For simplicity, I'm assuming the role already exists
          throw new Exception("Role 'Super User' does not exist. Please create the role before seeding the user.");
        }

        _context.UserRole.Add(new UserRole { RoleId = role.Id, UserId = newUser.Id });
        _context.SaveChanges();

        if (!result.Succeeded)
        {
          // Handle role assignment failure, if any
          throw new Exception($"Role assignment failed: {string.Join(", ", result.Errors)}");
        }
      }
    }
  }
}
