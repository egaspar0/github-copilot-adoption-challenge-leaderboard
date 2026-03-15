using LeaderboardApp.Models;
using LeaderboardApp.Services;
using LeaderboardApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeaderboardApp.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IAdminService adminService, ILogger<AdminController> logger)
        {
            _adminService = adminService;
            _logger = logger;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Guard helpers
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>Redirects to Home if the caller is not an admin. Returns null when access is allowed.</summary>
        private IActionResult? RequireAdmin()
        {
            if (!_adminService.IsAdminUser())
                return RedirectToAction("Index", "Home");
            return null;
        }

        /// <summary>Returns an error redirect when team reorganization is locked.</summary>
        private IActionResult? RequireNotStarted(string returnAction)
        {
            if (_adminService.IsTeamReorgLocked())
            {
                TempData["Error"] = "Team reorganization is locked. Set TeamReorgLocked=false in config to re-enable.";
                return RedirectToAction(returnAction);
            }
            return null;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Dashboard
        // ─────────────────────────────────────────────────────────────────────

        public async Task<IActionResult> Index()
        {
            if (RequireAdmin() is { } forbidden) return forbidden;

            ViewBag.IsLocked = _adminService.IsTeamReorgLocked();
            ViewBag.ParticipantCount = (await _adminService.GetAllParticipantsWithTeamsAsync()).Count;
            ViewBag.TeamCount = (await _adminService.GetAllTeamsAsync()).Count;
            return View();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Participants
        // ─────────────────────────────────────────────────────────────────────

        public async Task<IActionResult> Participants()
        {
            if (RequireAdmin() is { } forbidden) return forbidden;

            var vm = new AdminParticipantsViewModel
            {
                Participants = await _adminService.GetAllParticipantsWithTeamsAsync(),
                AllTeams = await _adminService.GetAllTeamsAsync(),
                IsLocked = _adminService.IsTeamReorgLocked()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveParticipant(Guid participantId, Guid newTeamId)
        {
            if (RequireAdmin() is { } forbidden) return forbidden;
            if (RequireNotStarted("Participants") is { } locked) return locked;

            try
            {
                await _adminService.MoveParticipantAsync(participantId, newTeamId);
                TempData["Success"] = "Participant moved successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moving participant {ParticipantId}", participantId);
                TempData["Error"] = $"Failed to move participant: {ex.Message}";
            }

            return RedirectToAction("Participants");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnassignParticipant(Guid participantId)
        {
            if (RequireAdmin() is { } forbidden) return forbidden;
            if (RequireNotStarted("Participants") is { } locked) return locked;

            try
            {
                await _adminService.UnassignParticipantAsync(participantId);
                TempData["Success"] = "Participant unassigned from team.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unassigning participant {ParticipantId}", participantId);
                TempData["Error"] = $"Failed to unassign participant: {ex.Message}";
            }

            return RedirectToAction("Participants");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteParticipant(Guid participantId)
        {
            if (RequireAdmin() is { } forbidden) return forbidden;
            if (RequireNotStarted("Participants") is { } locked) return locked;

            try
            {
                await _adminService.DeleteParticipantAsync(participantId);
                TempData["Success"] = "Participant deleted.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting participant {ParticipantId}", participantId);
                TempData["Error"] = $"Failed to delete participant: {ex.Message}";
            }

            return RedirectToAction("Participants");
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Teams
        // ─────────────────────────────────────────────────────────────────────

        public async Task<IActionResult> Teams()
        {
            if (RequireAdmin() is { } forbidden) return forbidden;

            var vm = new AdminTeamsViewModel
            {
                Teams = await _adminService.GetAllTeamsAsync(),
                IsLocked = _adminService.IsTeamReorgLocked()
            };
            return View(vm);
        }

        [HttpGet]
        public IActionResult CreateTeam()
        {
            if (RequireAdmin() is { } forbidden) return forbidden;
            if (_adminService.IsTeamReorgLocked())
            {
                TempData["Error"] = "Team reorganization is locked. Set TeamReorgLocked=false in config to re-enable.";
                return RedirectToAction("Teams");
            }
            return View(new Team());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTeam(Team team)
        {
            if (RequireAdmin() is { } forbidden) return forbidden;
            if (RequireNotStarted("Teams") is { } locked) return locked;

            if (!ModelState.IsValid)
                return View(team);

            try
            {
                await _adminService.CreateTeamAdminAsync(team);
                TempData["Success"] = $"Team '{team.Name}' created successfully.";
                return RedirectToAction("Teams");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating team");
                ModelState.AddModelError(string.Empty, $"Failed to create team: {ex.Message}");
                return View(team);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditTeam(Guid teamId)
        {
            if (RequireAdmin() is { } forbidden) return forbidden;
            if (_adminService.IsTeamReorgLocked())
            {
                TempData["Error"] = "Team reorganization is locked. Set TeamReorgLocked=false in config to re-enable.";
                return RedirectToAction("Teams");
            }

            var teams = await _adminService.GetAllTeamsAsync();
            var team = teams.FirstOrDefault(t => t.Teamid == teamId);
            if (team == null)
            {
                TempData["Error"] = "Team not found.";
                return RedirectToAction("Teams");
            }

            return View(team);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTeam(Guid teamId, Team team)
        {
            if (RequireAdmin() is { } forbidden) return forbidden;
            if (RequireNotStarted("Teams") is { } locked) return locked;

            team.Teamid = teamId;

            if (!ModelState.IsValid)
                return View(team);

            try
            {
                await _adminService.UpdateTeamAdminAsync(team);
                TempData["Success"] = $"Team '{team.Name}' updated successfully.";
                return RedirectToAction("Teams");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating team {TeamId}", teamId);
                ModelState.AddModelError(string.Empty, $"Failed to update team: {ex.Message}");
                return View(team);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTeam(Guid teamId)
        {
            if (RequireAdmin() is { } forbidden) return forbidden;
            if (RequireNotStarted("Teams") is { } locked) return locked;

            try
            {
                await _adminService.DeleteTeamAdminAsync(teamId);
                TempData["Success"] = "Team deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting team {TeamId}", teamId);
                TempData["Error"] = $"Failed to delete team: {ex.Message}";
            }

            return RedirectToAction("Teams");
        }

        // ─────────────────────────────────────────────────────────────────────
        //  GitHub handle (always available - not gated by ChallengeStarted)
        // ─────────────────────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetGitHubHandle(Guid participantId, string? githubHandle)
        {
            if (RequireAdmin() is { } forbidden) return forbidden;

            try
            {
                await _adminService.SetParticipantGitHubHandleAsync(participantId, githubHandle);
                TempData["Success"] = string.IsNullOrWhiteSpace(githubHandle)
                    ? "GitHub handle cleared."
                    : $"GitHub handle set to '{githubHandle.Trim()}'";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting GitHub handle for participant {ParticipantId}", participantId);
                TempData["Error"] = $"Failed to update GitHub handle: {ex.Message}";
            }

            return RedirectToAction("Participants");
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Sync (always available - read-only comparison)
        // ─────────────────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Sync()
        {
            if (RequireAdmin() is { } forbidden) return forbidden;

            var reports = await _adminService.SyncGitHubTeamsAsync();
            var vm = new SyncResultViewModel
            {
                Reports = reports,
                GitHubDisabled = !HttpContext.RequestServices
                    .GetRequiredService<IConfiguration>()
                    .GetValue<bool>("GitHubSettings:Enabled", false)
            };

            return View(vm);
        }
    }
}
