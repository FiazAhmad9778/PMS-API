namespace PMS.API.Application.Common.Security;

public static class Roles
{
  public const string SuperAdminRoleName = "Superadmin";
  public const string Technician = "Technician";

  public static readonly string[] Application = {
        SuperAdminRoleName,
        Technician,
    };
}
