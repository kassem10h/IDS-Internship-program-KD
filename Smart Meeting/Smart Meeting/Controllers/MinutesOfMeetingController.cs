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
    [Route("api/meeting/{meetingID}/minutes")]
    [ApiController]
    [Authorize]
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
        public async Task<ActionResult<MinutesOfMeetingDto>> GetMinutes(int meetingID)
        {
            var meetingExist = await _context.Meetings.FindAsync(meetingID);
            if (meetingExist == null) return NotFound("Meeting not found");

            var minutesMeeting = await _context.MinutesOfMeetings
                .Include(m => m.Author)
                .FirstOrDefaultAsync(m => m.MeetingID == meetingID);

            if (minutesMeeting == null) return NotFound("Minutes not found");

            var dtoResult = _mapper.Map<MinutesOfMeetingDto>(minutesMeeting);
            return Ok(dtoResult);
        }

        [HttpPost]
        public async Task<ActionResult> AddMinutes(int meetingID, MinutesOfMeetingDto minMeeting)
        {
            var meetingExist = await _context.Meetings.FindAsync(meetingID);
            if (meetingExist == null) return NotFound("Meeting not found");

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            var minOfMeetingExist = await _context.MinutesOfMeetings
                .FirstOrDefaultAsync(m => m.MeetingID == meetingID);

            if (minOfMeetingExist != null)
                return Conflict("Minutes of meeting already exist");

            var newMinOfMeeting = _mapper.Map<MinutesOfMeeting>(minMeeting);
            newMinOfMeeting.MeetingID = meetingID;
            newMinOfMeeting.AuthorId = currentUserId; // Set current user as author

            _context.MinutesOfMeetings.Add(newMinOfMeeting);
            await _context.SaveChangesAsync();

            return Ok("Minutes added successfully");
        }

        [HttpPut]
        public async Task<ActionResult> UpdateMinutes(int meetingID, MinutesOfMeetingDto minMeeting)
        {
            var meetingExist = await _context.Meetings.FindAsync(meetingID);
            if (meetingExist == null) return NotFound("Meeting not found");

            var minOfMeetingExist = await _context.MinutesOfMeetings
                .FirstOrDefaultAsync(m => m.MeetingID == meetingID);

            if (minOfMeetingExist == null)
                return NotFound("Minutes not found");

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");

            // Convert currentUserId to int for comparison  
            if (!int.TryParse(currentUserId, out var currentUserIdInt))
            {
                return Unauthorized();
            }

            // Only author or admin can update minutes  
            if (!isAdmin && minOfMeetingExist.AuthorId != currentUserIdInt)
            {
                return Forbid();
            }

            _mapper.Map(minMeeting, minOfMeetingExist);
            await _context.SaveChangesAsync();

            return Ok("Minutes updated successfully");
        }
    }
}
