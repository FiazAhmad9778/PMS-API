namespace PMS.API.Application.Common.Constants;

public static class ApplicationErrorConstants
{
  public const string GeneralError = "Something went wrong on server, Please reach out to support and refer to this error: ";

  #region Auth0 response

  public const string Auth0UserAreadyExist = "User with this email already exists.";
  public const string Auth0UserFailed = "User creation failed.";

  #endregion

  public const string InvoiceNonPaidMessage = $"Please pay your Non Paid invoice, or contact support for further details";
}
