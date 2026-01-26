using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using PMS.API.Core.Domain.Entities.Identity;
using PMS.API.Infrastructure.Data;

namespace PMS.API.Application.Identity;
public class ClaimApplication : IClaimApplication
{
  private readonly AppDbContext _context;

  public ClaimApplication(AppDbContext context)
  {
    _context = context;
  }
  public List<ApplicationClaim> GetList()
  {
    // Get all claims from the claims principal
    var allClaims = _context.ApplicationClaims.ToList();

    return allClaims;
    //return new List<ApplicationClaim>();
  }
}
