using LeaderboardApp.Models;
using LeaderboardApp.ViewModels;

namespace LeaderboardApp.Services
{
    public interface IAdminService
    {
        /// <summary>Returns true if the currently authenticated user is in the ChallengeSettings:Admin list.</summary>
        bool IsAdminUser();

        /// <summary>Returns true when ChallengeSettings:ChallengeStarted is true (reorganization is locked).</summary>
        bool IsChallengeStarted();

        Task<List<AdminParticipantViewModel>> GetAllParticipantsWithTeamsAsync();
        Task<List<Team>> GetAllTeamsAsync();

        /// <summary>Moves a participant to a new team in the DB and updates GitHub membership.</summary>
        Task MoveParticipantAsync(Guid participantId, Guid newTeamId);

        /// <summary>Removes a participant from their team (DB + GitHub) without deleting them.</summary>
        Task UnassignParticipantAsync(Guid participantId);

        /// <summary>Removes GitHub membership, deletes participant scores, then deletes the participant.</summary>
        Task DeleteParticipantAsync(Guid participantId);

        /// <summary>Creates a new team in the DB and on GitHub (if enabled).</summary>
        Task<Team> CreateTeamAdminAsync(Team team);

        /// <summary>Updates an existing team's details in the DB. Logs a warning if GitHubSlug changes.</summary>
        Task UpdateTeamAdminAsync(Team team);

        /// <summary>Unassigns all participants, removes GitHub team, deletes the team from DB.</summary>
        Task DeleteTeamAdminAsync(Guid teamId);

        /// <summary>
        /// Compares GitHub team memberships against DB for every team with a GitHubSlug.
        /// Returns a report per team listing discrepancies - no changes are made.
        /// </summary>
        Task<List<TeamSyncReport>> SyncGitHubTeamsAsync();
    }
}
