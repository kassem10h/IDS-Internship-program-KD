using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Smart_Meeting.DTOs;
using Smart_Meeting.Models;
using SmartMeeting.Data;
using SmartMeeting.DTOs;
using SmartMeeting.Models;

namespace SmartMeeting.Controllers
{
    [Route("api/room")]
    [ApiController]
    [Authorize]
    public class RoomController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public RoomController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<RoomDto>> AddRoom(RoomDto room)
        {
            var roomExist = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomName == room.RoomName);
            if (roomExist != null) return Conflict("Room already exists");

            var newRoom = _mapper.Map<Room>(room);
            _context.Rooms.Add(newRoom);
            await _context.SaveChangesAsync();

            var dtoResult = _mapper.Map<RoomDto>(newRoom);
            return CreatedAtAction(nameof(GetRoom), new { id = newRoom.ID }, dtoResult);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomDto>>> GetRooms()
        {
            var rooms = await _context.Rooms
                .Include(r => r.RoomFeatures)
                .ToListAsync();
            var dtoResult = _mapper.Map<List<RoomDto>>(rooms);
            return Ok(dtoResult);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RoomDto>> GetRoom(int id)
        {
            var room = await _context.Rooms
                .Include(r => r.RoomFeatures)
                .FirstOrDefaultAsync(r => r.ID == id);

            if (room == null) return NotFound();

            var dtoResult = _mapper.Map<RoomDto>(room);
            return Ok(dtoResult);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateRoom(int id, UpdateRoomDto room)
        {
            var roomExist = await _context.Rooms.FindAsync(id);
            if (roomExist == null) return NotFound();

            _mapper.Map(room, roomExist);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound();

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
