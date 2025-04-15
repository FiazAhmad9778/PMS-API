namespace PMS.API.Web.Common;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

public class LowercaseControllerNameTransformConvention : IControllerModelConvention
{
  public void Apply(ControllerModel controller)
  {
    controller.ControllerName = controller.ControllerName.ToLowerInvariant();
  }
}
