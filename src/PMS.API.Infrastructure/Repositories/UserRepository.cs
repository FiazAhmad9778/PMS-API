using PMS.API.Core.Domain.Entities.Identity;
using PMS.API.Core.Domain.Interfaces.Repositories;
using PMS.API.Infrastructure.Data;

namespace PMS.API.Infrastructure.Repositories;
public class UserRepository : EfRepository<User>, IUserRepository
{
  private readonly AppDbContext _context;
  public UserRepository(AppDbContext context) : base(context)
  {
    _context = context;
  }
}
