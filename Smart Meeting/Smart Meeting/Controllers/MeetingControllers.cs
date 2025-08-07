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
    [Route("api/meeting")]
    [ApiController]
    [Authorize]
    public class MeetingControllers : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public MeetingControllers(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MeetingDto>>> GetMeetings()
        {
            var meetings = await _context.Meetings
                .Include(m => m.Room)
                .Include(m => m.Employee)
                .Include(m => m.Attendees)
                .ToListAsync();

            var dtoResult = _mapper.Map<List<MeetingDto>>(meetings);
            return Ok(dtoResult);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MeetingDto>> GetMeeting(int id)
        {
            var meeting = await _context.Meetings
                .Include(m => m.Room)
                .Include(m => m.Employee)
                .Include(m => m.Attendees)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (meeting == null)
                return NotFound();

            var dtoResult = _mapper.Map<MeetingDto>(meeting);
            return Ok(dtoResult);
        }

        [HttpPost]
        public async Task<ActionResult<CreateMeetingDto>> CreateMeeting(CreateMeetingDto meeting)
        {
            // Check if room is available at the specified time
            var meetingExist = await _context.Meetings
                .FirstOrDefaultAsync(m => m.Date.Date == meeting.Date.Date &&
                                         m.RoomID == meeting.RoomID &&
                                         Math.Abs((m.Date - meeting.Date).TotalHours) < 2);

            if (meetingExist != null)
                return Conflict("Meeting at this date and room exists or conflicts with existing meeting");

            // Verify room exists
            var room = await _context.Rooms.FindAsync(meeting.RoomID);
            if (room == null || !room.IsAvailable)
                return BadRequest("Room not available");

            // Verify employee exists
            var employee = await _context.Users.FindAsync(meeting.EmployeeID);
            if (employee == null || !employee.IsActive)
                return BadRequest("Employee not found");

            var newMeeting = _mapper.Map<Meeting>(meeting);
            _context.Meetings.Add(newMeeting);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMeeting), new { id = newMeeting.ID }, meeting);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMeeting(int id, CreateMeetingDto meeting)
        {
            var existMeeting = await _context.Meetings.FindAsync(id);
            if (existMeeting == null)
                return NotFound();

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");

            // Only meeting organizer or admin can update
            if (!isAdmin && existMeeting.EmployeeID.ToString() != currentUserId)
            {
                return Forbid();
            }

            _mapper.Map(meeting, existMeeting);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMeeting(int id)
        {
            var meeting = await _context.Meetings.FindAsync(id);
            if (meeting == null)
                return NotFound();

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");

            // Only meeting organizer or admin can delete
            if (!isAdmin && meeting.EmployeeID.ToString() != currentUserId)
            {
                return Forbid();
            }

            _context.Meetings.Remove(meeting);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
