using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace LeaderboardApp.Models;

public partial class GhcacDbContext : DbContext
{
    public GhcacDbContext()
    {
    }

    public GhcacDbContext(DbContextOptions<GhcacDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Activity> Activities { get; set; }

    public virtual DbSet<Leaderboardentry> Leaderboardentries { get; set; }

    public virtual DbSet<Participant> Participants { get; set; }

    public virtual DbSet<Participantscore> Participantscores { get; set; }
    
    public virtual DbSet<Team> Teams { get; set; }

    public virtual DbSet<Teamdailysummary> Teamdailysummaries { get; set; }

    public virtual DbSet<Challenge> Challenges { get; set; }

    public virtual DbSet<MetricsData> MetricsData { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Activity>(entity =>
        {
            entity.HasKey(e => e.Activityid).HasName("activities_pkey");

            entity.ToTable("activities");

            entity.Property(e => e.Activityid).HasColumnName("activityid");
            entity.Property(e => e.Frequency)
                .HasMaxLength(50)
                .HasColumnName("frequency");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Scope)
                .HasMaxLength(50)
                .HasColumnName("scope");
            entity.Property(e => e.Weight)
                .HasPrecision(10, 2)
                .HasColumnName("weight");
            entity.Property(e => e.Weighttype)
                .HasMaxLength(50)
                .HasColumnName("weighttype");
        });

        modelBuilder.Entity<Leaderboardentry>(entity =>
        {
            entity.HasKey(e => e.Leaderboardentryid).HasName("leaderboardentries_pkey");

            entity.ToTable("leaderboardentries");

            entity.Property(e => e.Leaderboardentryid)
                .ValueGeneratedNever()
                .HasColumnName("leaderboardentryid");
            entity.Property(e => e.Lastupdated).HasColumnName("lastupdated");
            entity.Property(e => e.Score).HasColumnName("score");
            entity.Property(e => e.Teamid).HasColumnName("teamid");
            entity.Property(e => e.Teamname)
                .HasColumnType("character varying")
                .HasColumnName("teamname");

            entity.HasOne(d => d.Team).WithMany(p => p.Leaderboardentries)
                .HasForeignKey(d => d.Teamid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("leaderboardentries_teamid_fkey");
        });

        modelBuilder.Entity<Participant>(entity =>
        {
            entity.HasKey(e => e.Participantid).HasName("participants_pkey");

            entity.ToTable("participants");

            entity.HasIndex(e => e.Email, "uq_participants_email").IsUnique();

            entity.Property(e => e.Participantid)
                .ValueGeneratedNever()
                .HasColumnName("participantid");
            entity.Property(e => e.Email)
                .HasColumnType("character varying")
                .HasColumnName("email");
            entity.Property(e => e.Externalid).HasColumnName("externalid");
            entity.Property(e => e.Firstname)
                .HasColumnType("character varying")
                .HasColumnName("firstname");
            entity.Property(e => e.Githubhandle)
                .HasColumnType("character varying")
                .HasColumnName("githubhandle");
            entity.Property(e => e.Lastlogin).HasColumnName("lastlogin");
            entity.Property(e => e.Lastname)
                .HasColumnType("character varying")
                .HasColumnName("lastname");
            entity.Property(e => e.Mslearnhandle)
                .HasColumnType("character varying")
                .HasColumnName("mslearnhandle");
            entity.Property(e => e.Nickname)
                .HasColumnType("character varying")
                .HasColumnName("nickname");
            entity.Property(e => e.Passcode)
                .HasMaxLength(6)
                .HasColumnName("passcode");
            entity.Property(e => e.Passcodeexpiration).HasColumnName("passcodeexpiration");
            entity.Property(e => e.Refreshtoken)
                .HasColumnType("character varying")
                .HasColumnName("refreshtoken");
            entity.Property(e => e.Teamid).HasColumnName("teamid");

            entity.HasOne(d => d.Team).WithMany(p => p.Participants)
                .HasForeignKey(d => d.Teamid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("participants_teamid_fkey");
        });

        modelBuilder.Entity<Participantscore>(entity =>
        {
            entity.HasKey(e => e.Scoreid).HasName("participantscores_pkey");

            entity.ToTable("participantscores");

            entity.Property(e => e.Scoreid).HasColumnName("scoreid");
            entity.Property(e => e.Activityid).HasColumnName("activityid");
            entity.Property(e => e.Challengeid).HasColumnName("challengeid");
            entity.Property(e => e.Participantid).HasColumnName("participantid");
            entity.Property(e => e.Teamid).IsRequired(false).HasColumnName("teamid"); // nullable – nulled on unassign, restamped on move
            entity.Property(e => e.Score)
                .HasPrecision(10, 2)
                .HasColumnName("score");
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("timestamp");
            entity.Property(e => e.Validationlink)
                .HasColumnType("character varying")
                .HasColumnName("validationlink");

            entity.HasOne(d => d.Participant).WithMany(p => p.Participantscores)
                .HasForeignKey(d => d.Participantid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("participantscores_participantid_fkey");

            entity.HasOne(d => d.Activity).WithMany()
                .HasForeignKey(d => d.Activityid)
                .HasConstraintName("participantscores_activityid_fkey");

            entity.HasOne(d => d.Challenge).WithMany()
                .HasForeignKey(d => d.Challengeid)
                .HasConstraintName("participantscores_challengeid_fkey");

            entity.HasOne(d => d.Team).WithMany()
                .HasForeignKey(d => d.Teamid)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("participantscores_teamid_fkey");
        });


        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(e => e.Teamid).HasName("teams_pkey");

            entity.ToTable("teams");

            entity.Property(e => e.Teamid)
                .ValueGeneratedNever()
                .HasColumnName("teamid");
            entity.Property(e => e.Icon)
                .HasColumnType("character varying")
                .HasColumnName("icon");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
            entity.Property(e => e.Tagline)
                .HasColumnType("character varying")
                .HasColumnName("tagline");
            entity.Property(e => e.GitHubSlug)
                .HasColumnType("character varying")
                .HasColumnName("githubslug");
        });

        modelBuilder.Entity<MetricsData>(entity =>
        {
            entity.HasKey(e => e.MetricsId).HasName("metricsdata_pkey");

            entity.ToTable("metricsdata");

            entity.Property(e => e.MetricsId)
                .ValueGeneratedOnAdd() // Updated to reflect the identity column
                .HasColumnName("metricsid");

            entity.Property(e => e.Date)
                .IsRequired() // Ensures the column is NOT NULL
                .HasColumnName("date");

            entity.Property(e => e.OrgName)                
                .HasColumnName("orgname");

            entity.Property(e => e.JsonResponse)
                .IsRequired() // Ensures the column is NOT NULL
                .HasColumnType("jsonb") // For PostgreSQL
                .HasColumnName("jsonresponse");
        });


        modelBuilder.Entity<Teamdailysummary>(entity =>
        {
            entity.HasKey(e => e.Summaryid).HasName("teamdailysummaries_pkey");

            entity.ToTable("teamdailysummaries");

            entity.Property(e => e.Summaryid).HasColumnName("summaryid");
            entity.Property(e => e.Day).HasColumnName("day");
            entity.Property(e => e.Teamid).HasColumnName("teamid");
            entity.Property(e => e.Totalacceptancescount).HasColumnName("totalacceptancescount");
            entity.Property(e => e.Totalactivechatusers).HasColumnName("totalactivechatusers");
            entity.Property(e => e.Totalactiveusers).HasColumnName("totalactiveusers");
            entity.Property(e => e.Totalchatacceptances).HasColumnName("totalchatacceptances");
            entity.Property(e => e.Totalchatturns).HasColumnName("totalchatturns");
            entity.Property(e => e.Totallinesaccepted).HasColumnName("totallinesaccepted");
            entity.Property(e => e.Totallinessuggested).HasColumnName("totallinessuggested");
            entity.Property(e => e.Totalsuggestionscount).HasColumnName("totalsuggestionscount");

            entity.HasOne(d => d.Team).WithMany(p => p.Teamdailysummaries)
                .HasForeignKey(d => d.Teamid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("teamdailysummaries_teamid_fkey");
        });

        modelBuilder.Entity<Challenge>()
            .HasOne<Activity>(c => c.Activity)
            .WithMany() 
            .HasForeignKey(c => c.ActivityId);

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
