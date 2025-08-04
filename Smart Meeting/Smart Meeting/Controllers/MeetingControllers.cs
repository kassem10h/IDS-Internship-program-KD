using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Smart_Meeting.Data;
using Smart_Meeting.DTOs;
using Smart_Meeting.Models;
using System;

namespace Smart_Meeting.Controllers
{
    [Route("api/meeting")]
    [ApiController]
    public class MeetingControllers : ControllerBase
    {
        private readonly AppDBContext _context;
        private readonly IMapper _mapper;
        public MeetingControllers(AppDBContext context , IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MeetingDto>>> GetMeetings()
        {
            var meetings = await _context.Meetings.ToListAsync();
            var dtoResult = _mapper.Map<List<MeetingDto>>(meetings); 
            return Ok(dtoResult);

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MeetingDto>> GetMeeting(int id)
        {
            var meeting = await _context.Meetings.FindAsync(id);
            if (meeting == null)
                return NotFound();

            var dtoResult = _mapper.Map<MeetingDto>(meeting);
            return Ok(dtoResult);

        }


        [HttpPost]
        public async Task<ActionResult<CreateMeetingDto>> CreateMeeing(CreateMeetingDto meeting)
        {
            var MeetingExist = await _context.Meetings
                  .FirstOrDefaultAsync(m => m.Date == meeting.Date && m.RoomID == meeting.RoomID);
            if (MeetingExist != null) return Conflict("Meeting at this date and room exist");

            var newMeeting = _mapper.Map<Meeting>(meeting);
            
            _context.Meetings.Add(newMeeting);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetMeeting), new { id = newMeeting.ID }, meeting);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMeeting(int id, CreateMeetingDto meeting)
        {
            var ExistMeeting = await _context.Meetings.FindAsync(id);
            if (ExistMeeting == null)
                return NotFound();

            _mapper.Map(meeting, ExistMeeting);
            await _context.SaveChangesAsync();
            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
                return NotFound();

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}

