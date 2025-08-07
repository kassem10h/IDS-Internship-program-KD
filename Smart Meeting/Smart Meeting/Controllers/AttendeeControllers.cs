using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Smart_Meeting.DTOs;
using Smart_Meeting.Models;
using SmartMeeting.Data;

namespace Smart_Meeting.Controllers
{
    [Route("api/{meetingID}/attendee")]
    [ApiController]
    public class AttendeeControllers : ControllerBase
    { 
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        public AttendeeControllers(ApplicationDbContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AttendeeDto>>> GetAttendees(int meetingID)
        {
            var MeetingExist = await _context.Meetings.FindAsync(meetingID);
            if (MeetingExist == null) return NotFound();

            var attendees = await _context.Attendees
           .Include(a => a.employee)
           .Where(a => a.MeetingID == meetingID)
           .ToListAsync();

            var attendeeDtos = _mapper.Map<List<AttendeeDto>>(attendees);

            return Ok(attendeeDtos);

        }


        [HttpPost]
        public async Task<ActionResult> AddAttendee(int meetingID,AttendeeDto attendee)
        {
            var MeetingExist = await _context.Meetings.FindAsync(meetingID);
            if (MeetingExist == null) return NotFound();

            var AttendeeExist = await _context.Attendees
                  .FirstOrDefaultAsync(a => a.EmployeeID == attendee.EmployeeID && a.MeetingID == meetingID);
            if (AttendeeExist != null) return Conflict("Employee is already invited");

            var newAttendee = _mapper.Map<Attendee>(attendee);
            newAttendee.MeetingID = meetingID;
            _context.Attendees.Add(newAttendee);
            await _context.SaveChangesAsync();
            return Ok();   
        }


        [HttpDelete("{empId}")]
        public async Task<IActionResult> DeleteAtendee(int meetingID, int empId )
        {
            var MeetingExist = await _context.Meetings.FindAsync(meetingID);
            if (MeetingExist == null) return NotFound();

            var AttendeeExist = await _context.Attendees
                 .FirstOrDefaultAsync(a => a.EmployeeID == empId && a.MeetingID == meetingID);
            if (AttendeeExist == null) return Conflict("Employee is not found");


            _context.Attendees.Remove(AttendeeExist);
            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
}
