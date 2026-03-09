using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LeaderboardApp.Models;

public partial class Team
{
    public Guid Teamid { get; set; }

    [Required(ErrorMessage = "Team name is required.")]
    [StringLength(100)]
    public string Name { get; set; }

    [Url(ErrorMessage = "Please enter a valid URL.")]
    public string? Icon { get; set; }

    [Required(ErrorMessage = "Tagline is required.")]
    [StringLength(250)]
    public string Tagline { get; set; }

    public string? GitHubSlug { get; set; }

    public virtual ICollection<Leaderboardentry> Leaderboardentries { get; set; } = new List<Leaderboardentry>();

    public virtual ICollection<Participant> Participants { get; set; } = new List<Participant>();

    public virtual ICollection<Teamdailysummary> Teamdailysummaries { get; set; } = new List<Teamdailysummary>();
}
