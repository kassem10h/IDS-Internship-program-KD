using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Smart_Meeting.DTOs;
using Smart_Meeting.Models;
using SmartMeeting.Data;
using SmartMeeting.DTOs;
using SmartMeeting.Models;
using System.Security.Claims;

namespace SmartMeeting.Controllers
{
    [Route("api/meeting/{meetingID}/attendee")]
    [ApiController]
    [Authorize]
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
            var meetingExist = await _context.Meetings.FindAsync(meetingID);
            if (meetingExist == null) return NotFound("Meeting not found");

            var attendees = await _context.Attendees
                .Include(a => a.employee)
                .Where(a => a.MeetingID == meetingID)
                .ToListAsync();

            var attendeeDtos = _mapper.Map<List<AttendeeDto>>(attendees);
            return Ok(attendeeDtos);
        }

        [HttpPost]
        public async Task<ActionResult> AddAttendee(int meetingID, AttendeeDto attendee)
        {
            var meetingExist = await _context.Meetings.FindAsync(meetingID);
            if (meetingExist == null) return NotFound("Meeting not found");

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");

            // Only meeting organizer or admin can add attendees
            if (!isAdmin && meetingExist.EmployeeID.ToString() != currentUserId) // Convert EmployeeID to string for comparison
            {
                return Forbid();
            }

            // Check if employee exists
            var employee = await _context.Users.FindAsync(attendee.EmployeeID);
            if (employee == null || !employee.IsActive)
                return BadRequest("Employee not found");

            var attendeeExist = await _context.Attendees
                .FirstOrDefaultAsync(a => a.EmployeeID == attendee.EmployeeID && a.MeetingID == meetingID);

            if (attendeeExist != null)
                return Conflict("Employee is already invited");

            var newAttendee = _mapper.Map<Attendee>(attendee);
            newAttendee.MeetingID = meetingID;
            _context.Attendees.Add(newAttendee);
            await _context.SaveChangesAsync();

            return Ok("Attendee added successfully");
        }

        [HttpDelete("{empId}")]
        public async Task<IActionResult> DeleteAttendee(int meetingID, int empId) // Change empId type to int
        {
            var meetingExist = await _context.Meetings.FindAsync(meetingID);
            if (meetingExist == null) return NotFound("Meeting not found");

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");

            // Only meeting organizer or admin can remove attendees
            if (!isAdmin && meetingExist.EmployeeID.ToString() != currentUserId) // Convert EmployeeID to string for comparison
            {
                return Forbid();
            }

            var attendeeExist = await _context.Attendees
                .FirstOrDefaultAsync(a => a.EmployeeID == empId && a.MeetingID == meetingID); // empId is now an int

            if (attendeeExist == null)
                return NotFound("Attendee not found");

            _context.Attendees.Remove(attendeeExist);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
