using AutoMapper;
using PMS.API.Application.Features.Documents.DTO;
using PMS.API.Application.Features.Users.DTO;
using PMS.API.Core.Domain.Entities;
using PMS.API.Core.Domain.Entities.Identity;

namespace roult.API.Application.Mappings;

public class AutomapperProfiles : Profile
{
  public AutomapperProfiles()
  {
    CreateMap<User, UserResponseDto>();
    CreateMap<Document, DocumentResponseDto>();
  }
}
