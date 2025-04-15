using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS.API.Application.Common.Helpers;
public static class MappingHelpers
{
  public static string MapTypeOfOrder(string parameter)
  {
    if (string.IsNullOrEmpty(parameter))
    {
      return parameter; // or handle null/empty case as needed
    }

    var normalizedParameter = parameter.ToUpperInvariant();

    return normalizedParameter switch
    {
      "C" => "Collection",
      "D" => "Delivery",
      "DC" => "Depot Call",
      _ => parameter // Return the parameter itself if it doesn't match any condition
    };
  }
}
