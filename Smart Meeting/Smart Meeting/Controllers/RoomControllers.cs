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
    [Route("api/room")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly AppDBContext _context;
        private readonly IMapper _mapper;

        public RoomController(AppDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;   
        }

        [HttpPost]
        public async Task<ActionResult<RoomDto>> AddRoom(RoomDto room)
        {
            var RoomExist = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomName == room.RoomName);
            if (RoomExist != null) return Conflict("Room already exists");
            var NewRoom = _mapper.Map<Room>(room);
            _context.Rooms.Add(NewRoom);
            await _context.SaveChangesAsync();
            var dtoResult = _mapper.Map<RoomDto>(NewRoom);
            return CreatedAtAction(nameof(GetRoom), new { id = NewRoom.ID }, NewRoom);
        }

        [HttpGet] //gets all rooms 
        public async Task<ActionResult<IEnumerable<RoomDto>>> GetRooms()
        {
            var rooms = await _context.Rooms.ToListAsync();
            var dtoResult = _mapper.Map<List<RoomDto>>(rooms);
            return Ok(dtoResult);
        }

        [HttpGet("{id}")] //get one room according to id in the url
        public async Task<ActionResult<RoomDto>> GetRoom(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound();
            var dtoResult = _mapper.Map<RoomDto>(room);
            
            return Ok(dtoResult);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateRoom(int id, UpdateRoomDto room)
        {
            var roomExist = await _context.Rooms.FindAsync(id);
            if (roomExist == null ) return BadRequest();

            _mapper.Map(room, roomExist);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetRoom), new { id = roomExist.ID }, room);
        }

        [HttpDelete("{id}")]
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