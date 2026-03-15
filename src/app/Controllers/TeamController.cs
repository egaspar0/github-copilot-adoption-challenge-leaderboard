using LeaderboardApp.Models;
using LeaderboardApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LeaderboardApp.Controllers
{
    [Authorize]
    public class TeamController : Controller
    {
        private readonly GhcacDbContext _context;
        private readonly ILogger<TeamController> _logger;
        private readonly GitHubService _gitHubService;
        private readonly ChallengesService _challengesService;
        private readonly ScoringService _scoringService;
        private readonly IConfiguration _configuration;
        private readonly string _scope = "team";

        public TeamController(
            GhcacDbContext context,
            ILogger<TeamController> logger,
            GitHubService gitHubService,
            ChallengesService challengesService,
            ScoringService scoringService,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _gitHubService = gitHubService;
            _challengesService = challengesService;
            _scoringService = scoringService;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult AddTeam()
        {
            return View(new Team());
        }

        [HttpPost]
        public async Task<IActionResult> AddTeam([FromBody] Team team)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid data.");
            }

            var githubEnabled = _configuration.GetValue<bool>("GitHubSettings:Enabled", true);

            if (githubEnabled)
            {
                // Create team in GitHub
                team.GitHubSlug = await _gitHubService.CreateTeamAsync(team.Name);
                if (string.IsNullOrWhiteSpace(team.GitHubSlug))
                {
                    return BadRequest("Failed to create team in GitHub. Ensure your team name is Unique.");
                }
            }
            else
            {
                // Skip GitHub operations entirely
                team.GitHubSlug = "notavailable"; // explicit for clarity
                _logger.LogInformation("GitHub integration disabled. Creating team '{TeamName}' only in local database.", team.Name);
            }

            // Create and save to DB
            team.Teamid = Guid.NewGuid();
            team.Icon = team.Icon ?? "";
            _context.Teams.Add(team);

            var leaderboardEntry = new Leaderboardentry
            {
                Leaderboardentryid = Guid.NewGuid(),
                Teamid = team.Teamid,
                Teamname = team.Name,
                Score = 0,
                Lastupdated = DateTime.UtcNow
            };
            _context.Leaderboardentries.Add(leaderboardEntry);

            await _context.SaveChangesAsync();

            var message = githubEnabled ? "Team created successfully (local + GitHub)!" : "Team created successfully (local only - GitHub disabled).";
            return Ok(new { message });
        }

        [HttpGet]
        public async Task<IActionResult> EditTeam(Guid teamId)
        {
            var team = await _context.Teams.FindAsync(teamId);
            if (team == null) return NotFound();

            return View(team);
        }

        [HttpPost]
        public async Task<IActionResult> EditTeam(Guid teamId, [FromBody] Team updatedTeam)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid data.");
            }

            var team = await _context.Teams.FindAsync(teamId);
            if (team == null) return NotFound();

            // Update team properties
            team.Name = updatedTeam.Name;
            team.Icon = updatedTeam.Icon ?? "";
            team.Tagline = updatedTeam.Tagline;

            _context.Teams.Update(team);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Team updated successfully!" });
        }

        [HttpGet]
        public async Task<IActionResult> Score(Guid teamId, bool refresh = false)
        {
            var team = await _context.Teams.FindAsync(teamId);
            if (team == null) return NotFound();

            // Refresh logic if requested
            if (refresh)
            {
                if (team.GitHubSlug != null)
                {
                    await _scoringService.InsertTeamGitHubScoresAsync(team.GitHubSlug);
                }
                await _challengesService.UpdateLeaderboardAsync(teamId);
            }

            // Fetch scores
            var scores = await _context.Participantscores
                .Where(ps => ps.Teamid == teamId)
                .OrderByDescending(ps => ps.Timestamp)
                .Select(ps => new TeamScoreEntryViewModel
                {
                    Nickname = ps.Participant.Nickname,
                    Score = ps.Score,
                    ActivityName = $"{ps.Activity.Name}:{ps.Challenge.Title}",
                    Timestamp = ps.Timestamp,
                    IsTeamActivity = string.Equals(ps.Activity.Scope, _scope, StringComparison.OrdinalIgnoreCase)
                })
                .ToListAsync();

            // Fetch participant nicknames
            var participantNicknames = await _context.Participants
                .Where(p => p.Teamid == teamId)
                .OrderBy(p => p.Nickname)
                .Select(p => p.Nickname)
                .ToListAsync();

            // Set ViewBag properties
            ViewBag.TeamName = team.Name;
            ViewBag.TeamTagline = team.Tagline;
            ViewBag.TeamIcon = team.Icon;
            ViewBag.TeamId = team.Teamid;
            ViewBag.ParticipantNicknames = participantNicknames;
            ViewBag.TotalScore = _context.Leaderboardentries.Where(l => l.Teamid == teamId).FirstOrDefault()?.Score;

            return View(scores);
        }      
    }
}
