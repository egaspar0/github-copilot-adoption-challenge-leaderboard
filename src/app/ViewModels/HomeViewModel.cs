using LeaderboardApp.DTOs;

namespace LeaderboardApp.ViewModels
{
    public class HomeViewModel
    {
        public List<LeaderboardEntryDto> Leaderboard { get; set; } = [];
        public List<ChallengeDto>? Challenges { get; set; }
    }
}
