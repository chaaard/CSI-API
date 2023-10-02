using AutoMapper;
using CSI.Application.DTOs;
using CSI.Domain.Entities;

namespace CSI.API.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>();
        }
    }
}
