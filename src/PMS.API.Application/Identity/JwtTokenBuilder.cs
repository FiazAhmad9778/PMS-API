using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace PMS.API.Application.Identity;

public class JwtTokenBuilder
{
  private JwtOptions? _jwtOptions;
  private Dictionary<string, object> _claims = new();
  private IIdentity? _identity;

  public JwtTokenBuilder WithOption(JwtOptions options)
  {
    _jwtOptions = options;
    return this;
  }

  public JwtTokenBuilder WithClaimsIdentity(ClaimsIdentity claimsIdentity)
  {
    // Replaced the Method for handling the Duplicate Claims.
    // In case of Multiple Roles multiple claims were returning with same key need to handle that.

    var principalClaims =
        GetDictionaryFromClaimsIdentity(claimsIdentity);

    return WithClaims(principalClaims);
  }

  public JwtTokenBuilder WithClaimsPrincipal(ClaimsPrincipal principal)
  {
    _identity = principal.Identity;

    // Replaced the Method for handling the Duplicate Claims.
    // In case of Multiple Roles multiple claims were returning with same key need to handle that.

    var principalClaims =
        GetDictionaryFromClaimsPrincipal(principal);

    return WithClaims(principalClaims);
  }

  public Dictionary<string, object> GetDictionaryFromClaimsPrincipal(ClaimsPrincipal principal)
  {
    var claimDictionary = new Dictionary<string, object>();
    if (principal is not null && principal.Claims is not null)
    {
      foreach (var claim in principal.Claims)
      {
        if (claimDictionary.ContainsKey(claim.Type))
        {
          var currentvalue = claimDictionary[claim.Type];
          claimDictionary[claim.Type] = currentvalue + "," + claim.Value;
        }
        else
        {
          claimDictionary.Add(claim.Type, claim.Value);
        }
      }
    }
    return claimDictionary;
  }

  public Dictionary<string, object> GetDictionaryFromClaimsIdentity(ClaimsIdentity claimsIdentity)
  {
    var claimDictionary = new Dictionary<string, object>();

    if (claimsIdentity is not null && claimsIdentity.Claims is not null)
    {
      foreach (var claim in claimsIdentity.Claims)
      {
        if (claimDictionary.ContainsKey(claim.Type))
        {
          var currentvalue = claimDictionary[claim.Type];
          claimDictionary[claim.Type] = currentvalue + "," + claim.Value;
        }
        else
        {
          claimDictionary.Add(claim.Type, claim.Value);
        }
      }
    }
    return claimDictionary;
  }


  public JwtTokenBuilder WithClaims(IDictionary<string, object> claims)
  {
    claims.ToList().ForEach(claim => _claims.Add(claim.Key, claim.Value));
    return this;
  }

  public string Build()
  {
    if (_jwtOptions == null)
    {
      throw new ArgumentException("_jwtOptions not specified");
    }

    var credentials =
                new SigningCredentials(CreateSigningKey(_jwtOptions.Secret!), SecurityAlgorithms.HmacSha512Signature);

    var tokenDescriptor = new SecurityTokenDescriptor
    {
      Subject = _identity != null ? new ClaimsIdentity(_identity) : null,
      //Expires = DateTime.Now.AddMinutes(_jwtOptions.ExpireAfterMinute),
      Expires = DateTime.Now.AddDays(30),
      SigningCredentials = credentials,
      Audience = _jwtOptions.Audience,
      Issuer = _jwtOptions.Issuer,
      Claims = _claims
    };
    var tokenHandler = new JwtSecurityTokenHandler();

    var token = tokenHandler.CreateToken(tokenDescriptor);

    return tokenHandler.WriteToken(token);
  }

  public static SecurityKey CreateSigningKey(string secret)
  {
    return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
  }
}
