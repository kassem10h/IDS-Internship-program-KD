using AutoMapper;
using Microsoft.Extensions.Logging;
using Smart_Meeting.Models;

namespace Smart_Meeting.DTOs
{
    public class AutoMapper:Profile
    {

        public AutoMapper()
        {
            CreateMap<RoomFeatures, RoomFeaturesDto>();
            CreateMap<Room, RoomDto>();
            CreateMap<Room, UpdateRoomDto>();
            CreateMap<Employee, CreateEmployeeDto>();
            CreateMap<Employee, EmployeeDto>();

            CreateMap<RoomFeaturesDto, RoomFeatures>();
            CreateMap<RoomDto, Room>();
            CreateMap<UpdateRoomDto, Room>();
            CreateMap<CreateEmployeeDto ,Employee>();
            CreateMap<EmployeeDto ,Employee>();


        }

    }
}
