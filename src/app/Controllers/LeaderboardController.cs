using LeaderboardApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace LeaderboardApp.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LeaderboardController : ControllerBase
    {
        private readonly LeaderboardService _leaderboardService;

        public LeaderboardController(LeaderboardService leaderboardService)
        {
            _leaderboardService = leaderboardService;
        }

        [HttpGet]
        public async Task<IActionResult> GetLeaderboard()
        {
            var leaderboard = await _leaderboardService.GetLeaderboardAsync();
            return Ok(leaderboard);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateLeaderboard()
        {
            await _leaderboardService.UpdateLeaderboardAsync();
            return Ok("Leaderboard updated successfully.");
        }
    }
}