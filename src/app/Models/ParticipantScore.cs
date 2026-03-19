namespace LeaderboardApp.Models;

public partial class Participantscore
{
    public int Scoreid { get; set; }

    public Guid Participantid { get; set; }

    public int Activityid { get; set; }

    public int Challengeid { get; set; }

    public decimal Score { get; set; }

    public DateTime? Timestamp { get; set; }

    public virtual Participant Participant { get; set; } = null!;

    public string? Validationlink { get; set; }

    public Guid? Teamid { get; set; }

    public virtual Team? Team { get; set; }

    public virtual Activity Activity { get; set; } = null!;

    public virtual Challenge Challenge { get; set; } = null!;
}
