using System.Web;
using MediatR;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Auth.DTO;
using PMS.API.Application.Services.Interfaces;

namespace PMS.API.Application.Features.Auth.Commands.VerifyUser;

public class VerifyUserCommand : IRequest<ApplicationResult<UserVerificationResultDto>>
{
  public string? Token { get; set; }
}

public class
    VerifyCompanyCommandHandler : IRequestHandler<VerifyUserCommand,
        ApplicationResult<UserVerificationResultDto>>
{
  private readonly IIdentityService _identityService;
  private readonly IDataProtector _dataProtector;

  public VerifyCompanyCommandHandler(IDataProtectionProvider dataProtectionProvider,
      IIdentityService identityService)
  {
    _identityService = identityService;
    _dataProtector = dataProtectionProvider.CreateProtector(new DataProtectionTokenProviderOptions().Name);
  }

  public async Task<ApplicationResult<UserVerificationResultDto>> Handle(VerifyUserCommand request,
      CancellationToken cancellationToken)
  {
    var token = HttpUtility.UrlDecode(request.Token!);


    var resetTokenArray = Convert.FromBase64String(token!);
    var unprotectedResetTokenArray = _dataProtector.Unprotect(resetTokenArray);

    var userId = 0;
    await using (var ms = new MemoryStream(unprotectedResetTokenArray))
    {
      using (var reader = new BinaryReader(ms))
      {
        // Read off the creation UTC time stamp
        reader.ReadInt64();

        // Then you can read the userId!
        var userStringId = reader.ReadString();
        userId = Convert.ToInt32(userStringId);
      }
    }

    var user = await _identityService.GetById(userId);

    var resultDto = new UserVerificationResultDto()
    {
      Email = user.Email
    };

    var result = await _identityService.VerifyUserViaToken(user, token!);
    if (result)
    {
      return ApplicationResult<UserVerificationResultDto>.SuccessResult(resultDto);
    }

    return ApplicationResult<UserVerificationResultDto>.Error("The verification link you are using has expired.", resultDto);
  }
}
