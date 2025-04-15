namespace PMS.API.Core.Models;
public class ClaimGroupResponseModel
{
  public ClaimGroupResponseModel()
  {
    Claims = new List<ClaimResponseModel>();
  }
  public List<ClaimResponseModel> Claims { get; set; }
  public string? ClaimGroupName { get; set; }

}
public class ClaimResponseModel
{
  public string? ClaimCode { get; set; }
  public string? ClaimValue { get; set; }
  public bool IsAssigned { get; set; }
  public long ClaimGroupId { get; set; }
  public long ClaimId { get; set; }
  public long? RoleId { get; set; }
}
