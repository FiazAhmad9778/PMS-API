namespace PMS.API.Web.Common;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Text.RegularExpressions;

public class LowercaseControllerNameTransformConvention : IControllerModelConvention
{
  public void Apply(ControllerModel controller)
  {
    // Convert PascalCase to kebab-case
    // Example: ARDashboard -> ar-dashboard, Patient -> patient, Organization -> organization
    // Insert hyphen before capital letters that are followed by lowercase (but not at start)
    var kebabCase = Regex.Replace(
      controller.ControllerName,
      "(?<!^)([A-Z])(?=[a-z])",
      "-$1",
      RegexOptions.Compiled
    );
    
    // Also handle consecutive capitals: insert hyphen before capital if preceded by capital and followed by lowercase
    kebabCase = Regex.Replace(
      kebabCase,
      "([A-Z]+)([A-Z][a-z])",
      "$1-$2",
      RegexOptions.Compiled
    );
    
    controller.ControllerName = kebabCase.ToLowerInvariant();
  }
}
