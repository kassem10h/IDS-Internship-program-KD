using AutoMapper;
using SmartMeeting.DTOs;
using SmartMeeting.Models;

namespace SmartMeeting.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // User mappings
            CreateMap<ApplicationUser, UserInfoDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));

            CreateMap<RegisterDto, ApplicationUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email));

            // You can add more mappings here as needed
        }
    }
}
