using AutoMapper;
using IdentityService.Domain.Entities;
using IdentityService.Application.DTOs.Responses.Role;

namespace IdentityService.Application.Mappings;

public class RoleMappingProfile : Profile
{
    public RoleMappingProfile()
    {
        CreateMap<Role, RoleResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.Parse(src.Id)))
            .ForMember(dest => dest.Permissions, opt => opt.Ignore()); // Permissions not available in Role entity
    }
}
