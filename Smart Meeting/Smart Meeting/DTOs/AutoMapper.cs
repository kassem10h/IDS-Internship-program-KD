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
            CreateMap<Attendee, AttendeeDto>();
            CreateMap<MinutesOfMeeting, MinutesOfMeetingDto>();
            CreateMap<Meeting, CreateMeetingDto>();
            CreateMap<Meeting, MeetingDto>();
            CreateMap<Employee, LogInDto>();


            CreateMap<RoomFeaturesDto, RoomFeatures>();
            CreateMap<RoomDto, Room>();
            CreateMap<UpdateRoomDto, Room>();
            CreateMap<CreateEmployeeDto ,Employee>();
            CreateMap<EmployeeDto ,Employee>();
            CreateMap<AttendeeDto ,Attendee>();
            CreateMap<MinutesOfMeetingDto ,MinutesOfMeeting>();
            CreateMap<CreateMeetingDto ,Meeting>();
            CreateMap<MeetingDto ,Meeting>();
            CreateMap<LogInDto, Employee>();


        }

    }
}
