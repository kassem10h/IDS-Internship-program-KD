using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 
using Smart_Meeting.DTOs;
using Smart_Meeting.Models;
using SmartMeeting.Data;

namespace Smart_Meeting.Controllers
{
    [Route("api/{meetingID}/MoM")]
    [ApiController]
    public class MinutesOfMeetingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        public MinutesOfMeetingController(ApplicationDbContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MinutesOfMeetingDto>>> GetMOM(int meetingID)
        {
            var MeetingExist = await _context.Meetings.FindAsync(meetingID);
            if (MeetingExist == null) return NotFound();

            var MinsMeeting = await _context.MinutesOfMeetings
             .FirstOrDefaultAsync(M => M.MeetingID == meetingID);
            

            var dtoResult = _mapper.Map<MinutesOfMeetingDto>(MinsMeeting);

            return Ok(dtoResult);
        }


        [HttpPost]
        public async Task<ActionResult> AddMOM(int meetingID, MinutesOfMeetingDto MinMeeting)
        {
            var MeetingExist = await _context.Meetings.FindAsync(meetingID);
            if (MeetingExist == null) return NotFound();

            var MinOfMeetingExist = await _context.MinutesOfMeetings
                  .FirstOrDefaultAsync(m => m.MeetingID == meetingID);
            if (MinOfMeetingExist != null) return Conflict("Minutes of meeting are added");

            var newMinOfMeeting = _mapper.Map<MinutesOfMeeting>(MinMeeting);
            newMinOfMeeting.MeetingID = meetingID;
            _context.MinutesOfMeetings.Add(newMinOfMeeting);
            await _context.SaveChangesAsync();
            return Ok();
        }


        [HttpPut]
        public async Task<ActionResult<RoomFeaturesDto>> UpdateMOM(int meetingID, MinutesOfMeetingDto MinMeeting)
        {
            var MeetingExist = await _context.Meetings.FindAsync(meetingID);
            if (MeetingExist == null) return NotFound();

            var MinOfMeetingExist = await _context.MinutesOfMeetings
                 .FirstOrDefaultAsync(m => m.MeetingID == meetingID);
            if (MinOfMeetingExist == null) return Conflict("Minutes of meeting are not added");


            _mapper.Map(MinMeeting, MinOfMeetingExist);
            await _context.SaveChangesAsync();
            return Ok();

        }

    }
}
