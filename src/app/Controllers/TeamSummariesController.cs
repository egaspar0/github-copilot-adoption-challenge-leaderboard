using LeaderboardApp.DTOs;
using LeaderboardApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace LeaderboardApp.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TeamMetricsController : ControllerBase
    {
        private readonly GhcacDbContext _context;

        public TeamMetricsController(GhcacDbContext context)
        {
            _context = context;
        }

        // GET: api/teamsummaries/{teamId}
        [HttpGet("{teamId}")]
        public async Task<ActionResult<IEnumerable<TeamDailySummaryDto>>> GetTeamSummaries(Guid teamId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Teamdailysummaries.AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(ts => ts.Day >= DateOnly.FromDateTime(startDate.Value));
            }

            if (endDate.HasValue)
            {
                query = query.Where(ts => ts.Day <= DateOnly.FromDateTime(endDate.Value));
            }

            var summaries = await query
                .Where(ts => ts.Teamid == teamId)
                .OrderBy(ts => ts.Day)
                .Select(ts => new TeamDailySummaryDto
                {
                    Day = ts.Day.ToDateTime(TimeOnly.MinValue),
                    TotalSuggestionsCount = ts.Totalsuggestionscount,
                    TotalAcceptancesCount = ts.Totalacceptancescount,
                    TotalLinesSuggested = ts.Totallinessuggested,
                    TotalLinesAccepted = ts.Totallinesaccepted,
                    TotalActiveUsers = ts.Totalactiveusers,
                    TotalChatAcceptances = ts.Totalchatacceptances,
                    TotalChatTurns = ts.Totalchatturns,
                    TotalActiveChatUsers = ts.Totalactivechatusers
                })
                .ToListAsync();

            if (summaries == null || !summaries.Any())
            {
                return NotFound("No metrics found for this team.");
            }

            return Ok(summaries);
        }

        // POST: api/teamsummaries/{teamId}
        [HttpPost("{teamId}")]
        public async Task<ActionResult> AddTeamSummary(Guid teamId, [FromBody] TeamDailySummaryDto summaryDto)
        {
            var team = await _context.Teams.FindAsync(teamId);
            if (team == null)
            {
                return NotFound("Team not found.");
            }

            // Add summary entry
            var summary = new Teamdailysummary
            {
                Teamid = teamId,
                Day = DateOnly.FromDateTime(summaryDto.Day),
                Totalsuggestionscount = summaryDto.TotalSuggestionsCount,
                Totalacceptancescount = summaryDto.TotalAcceptancesCount,
                Totallinessuggested = summaryDto.TotalLinesSuggested,
                Totallinesaccepted = summaryDto.TotalLinesAccepted,
                Totalactiveusers = summaryDto.TotalActiveUsers,
                Totalchatacceptances = summaryDto.TotalChatAcceptances,
                Totalchatturns = summaryDto.TotalChatTurns,
                Totalactivechatusers = summaryDto.TotalActiveChatUsers
            };

            _context.Teamdailysummaries.Add(summary);
            await _context.SaveChangesAsync();

            // Update leaderboard based on the new summary
            //await UpdateLeaderboard(teamId);

            return Ok();
        }

        // Method to update leaderboard based on daily summaries
        private async Task UpdateLeaderboard(Guid teamId)
        {
            var teamScores = await _context.Teamdailysummaries
                                           .Where(ts => ts.Teamid == teamId)
                                           .SumAsync(ts => ts.Totalacceptancescount); // Example logic

            var leaderboardEntry = await _context.Leaderboardentries.FirstOrDefaultAsync(le => le.Teamid == teamId);

            if (leaderboardEntry != null)
            {
                leaderboardEntry.Score = teamScores;
                leaderboardEntry.Lastupdated = DateTime.UtcNow;
            }
            else
            {
                _context.Leaderboardentries.Add(new Leaderboardentry
                {
                    Leaderboardentryid = Guid.NewGuid(),
                    Teamid = teamId,
                    Teamname = (await _context.Teams.FindAsync(teamId)).Name,
                    Score = teamScores,
                    Lastupdated = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}
