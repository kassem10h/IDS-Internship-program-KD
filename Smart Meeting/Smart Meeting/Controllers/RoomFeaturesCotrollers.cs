using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 
using Smart_Meeting.DTOs;
using Smart_Meeting.Models;
using SmartMeeting.Data;

namespace Smart_Meeting.Controllers
{
    [Route("api/room/{roomId}/features")]
    [ApiController]

    public class RoomFeaturesCotrollers : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public RoomFeaturesCotrollers(ApplicationDbContext context, IMapper mapper) 
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<RoomFeaturesDto>> GetRoomFeatures(int roomId)
        {
            var RoomExist = await _context.Rooms.FindAsync(roomId);
            if(RoomExist == null) return NotFound();

            var roomFeatures = await _context.RoomFeatures
                .FirstOrDefaultAsync(rf => rf.RoomID == roomId);
            var dtoResult = _mapper.Map<RoomFeaturesDto>(roomFeatures);
            return Ok(dtoResult);
        }

        [HttpPost]

        public async Task<ActionResult<RoomFeaturesDto>> AddRoomFeatures(int roomId,RoomFeaturesDto roomFeatures)
        {
            var RoomExist = await _context.Rooms.FindAsync(roomId);
            if (RoomExist == null) return NotFound();

            var FeaturesExist = await _context.RoomFeatures
                .FirstOrDefaultAsync(rf => rf.RoomID == roomId);
            if(FeaturesExist != null) return Conflict("Features are already added to this room");
            
            var features = _mapper.Map<RoomFeatures>(roomFeatures);
            features.RoomID = roomId;
            
            _context.RoomFeatures.Add(features);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut]
        public async Task<ActionResult<RoomFeaturesDto>> UpdateRoomFeatures(int roomId, RoomFeaturesDto roomFeatures) 
        {
            var RoomExist = await _context.Rooms.FindAsync(roomId);
            if (RoomExist == null) return NotFound();

            var FeaturesExist = await _context.RoomFeatures
                .FirstOrDefaultAsync(rf => rf.RoomID == roomId);
            if (FeaturesExist == null) return NotFound();

            _mapper.Map(roomFeatures, FeaturesExist);
            await _context.SaveChangesAsync();
            return Ok();

        }

    }
}
