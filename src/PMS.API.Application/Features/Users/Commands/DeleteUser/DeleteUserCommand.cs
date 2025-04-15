using MediatR;
using PMS.API.Application.Common.Models;
using PMS.API.Core.Domain.Interfaces.Repositories;

namespace PMS.API.Application.Features.Users.Commands.DeleteUser;
public class DeleteUserCommand : IRequest<ApplicationResult<bool>>
{
  public long Id { get; set; }
}

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, ApplicationResult<bool>>
{
  private readonly IUserRepository _repository;
  public DeleteUserCommandHandler(IUserRepository repository)
  {
    _repository = repository;
  }

  public async Task<ApplicationResult<bool>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
  {
    var User = await _repository.GetByIdAsync(request.Id);
    if (User == null) return ApplicationResult<bool>.Error("User not found!");
    await _repository.DeleteAsync(request.Id);
    return ApplicationResult<bool>.SuccessResult(true);
  }
}
