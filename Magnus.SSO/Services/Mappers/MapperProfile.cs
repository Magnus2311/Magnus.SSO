using AutoMapper;
using Magnus.SSO.Database.Models;
using Magnus.SSO.Models.DTOs;
using MongoDB.Bson;

namespace Magnus.SSO.Services.Mappers
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<UserDTO, User>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.Id) ? ObjectId.GenerateNewId() : ObjectId.Parse(src.Id)));
            CreateMap<User, UserDTO>();
        }
    }
}
