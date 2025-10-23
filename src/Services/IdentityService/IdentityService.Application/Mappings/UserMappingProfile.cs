using AutoMapper;
using IdentityService.Domain.Entities;
using IdentityService.Contracts.Responses.User;

namespace IdentityService.Application.Mappings;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.Parse(src.Id)))
            .ForMember(dest => dest.Roles, opt => opt.Ignore()); // Will be populated separately

        CreateMap<User, UserResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.Parse(src.Id)))
            .ForMember(dest => dest.Roles, opt => opt.Ignore()); // Will be populated separately
    }
}
