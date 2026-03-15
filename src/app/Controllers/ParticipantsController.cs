using LeaderboardApp.Models;
using LeaderboardApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace LeaderboardApp.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ParticipantsController : ControllerBase
    {
        private readonly GhcacDbContext _context;
        private readonly IAdminService _adminService;

        public ParticipantsController(GhcacDbContext context, IAdminService adminService)
        {
            _context = context;
            _adminService = adminService;
        }

        // GET: api/participants
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Participant>>> GetParticipants()
        {
            if (!_adminService.IsAdminUser()) return Forbid();
            return await _context.Participants.ToListAsync();
        }

        // GET: api/participants/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Participant>> GetParticipant(Guid id)
        {
            if (!_adminService.IsAdminUser()) return Forbid();
            var participant = await _context.Participants.FindAsync(id);

            if (participant == null)
            {
                return NotFound();
            }

            return participant;
        }

        // POST: api/participants
        [HttpPost]
        public async Task<ActionResult<Participant>> CreateParticipant(Participant participant)
        {
            if (!_adminService.IsAdminUser()) return Forbid();
            _context.Participants.Add(participant);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetParticipant), new { id = participant.Participantid }, participant);
        }

        // PUT: api/participants/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateParticipant(Guid id, Participant participant)
        {
            if (!_adminService.IsAdminUser()) return Forbid();

            if (id != participant.Participantid)
            {
                return BadRequest();
            }

            _context.Entry(participant).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ParticipantExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/participants/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteParticipant(Guid id)
        {
            if (!_adminService.IsAdminUser()) return Forbid();

            var participant = await _context.Participants.FindAsync(id);
            if (participant == null)
            {
                return NotFound();
            }

            _context.Participants.Remove(participant);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ParticipantExists(Guid id)
        {
            return _context.Participants.Any(e => e.Participantid == id);
        }
    }

}
