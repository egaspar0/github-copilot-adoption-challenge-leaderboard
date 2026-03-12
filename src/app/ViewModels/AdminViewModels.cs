using LeaderboardApp.Models;

namespace LeaderboardApp.ViewModels
{
    /// <summary>Flat representation of a participant with their current team - used in Admin views.</summary>
    public class AdminParticipantViewModel
    {
        public Guid ParticipantId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Nickname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? GitHubHandle { get; set; }
        public Guid? TeamId { get; set; }
        public string? TeamName { get; set; }

        public string FullName => $"{FirstName} {LastName}";
    }

    /// <summary>View model for the Admin Participants page.</summary>
    public class AdminParticipantsViewModel
    {
        public List<AdminParticipantViewModel> Participants { get; set; } = new();
        public List<Team> AllTeams { get; set; } = new();

        /// <summary>True when ChallengeStarted = true; all mutation buttons are disabled.</summary>
        public bool IsLocked { get; set; }
    }

    /// <summary>View model for the Admin Teams page.</summary>
    public class AdminTeamsViewModel
    {
        public List<Team> Teams { get; set; } = new();
        public bool IsLocked { get; set; }
    }

    /// <summary>Per-team discrepancy report produced by the Sync operation.</summary>
    public class TeamSyncReport
    {
        public string TeamName { get; set; } = string.Empty;
        public string GitHubSlug { get; set; } = string.Empty;

        /// <summary>GitHub handles present on the GitHub team but NOT found in the DB for this team.</summary>
        public List<string> InGitHubNotInDb { get; set; } = new();

        /// <summary>GitHub handles found in the DB for this team but NOT present on the GitHub team.</summary>
        public List<string> InDbNotInGitHub { get; set; } = new();

        public bool HasDiscrepancies => InGitHubNotInDb.Any() || InDbNotInGitHub.Any();

        /// <summary>Non-null if an error occurred fetching GitHub data for this team.</summary>
        public string? Error { get; set; }
    }

    /// <summary>View model for the Sync Result page.</summary>
    public class SyncResultViewModel
    {
        public List<TeamSyncReport> Reports { get; set; } = new();
        public bool GitHubDisabled { get; set; }
    }
}
