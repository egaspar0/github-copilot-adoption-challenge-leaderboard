# Go Live Runbook

This document covers all steps required to reset test data and go live with the Copilot Challenge leaderboard.

## Prerequisites

- Azure CLI (`az`) — authenticated with access to `rg-github-copilot-challenge-prd`
- GitHub CLI (`gh`) — authenticated as an org owner/admin of `racwa`
- Access to Azure SQL Query Editor (via Azure Portal) or `sqlcmd`

---

## 1. Reset Database

Run in Azure Portal → SQL Database `prd-sql-eyj6v` → **Query Editor**, or via `sqlcmd`.

> Run in this exact order to respect foreign key constraints. Challenges and Teams are preserved.

```sql
-- Clear scoring and activity data (FK-safe order)
DELETE FROM Participantscores;
DELETE FROM Leaderboardentries;
DELETE FROM Teamdailysummaries;
DELETE FROM MetricsData;
DELETE FROM Activities;
DELETE FROM Participants;
```

If you want to also wipe teams (and re-create them fresh from the app):

```sql
DELETE FROM Teams;
```

> **Note:** If Teams are deleted, participants will need to create their teams again via the app before the challenge starts.

---

## 2. Reset GitHub Team Memberships

Removes all members from every sub-team under the `copilot-challenge` parent team.

```powershell
gh api /orgs/racwa/teams/copilot-challenge/teams --paginate --jq '.[].slug' | ForEach-Object {
    $slug = $_
    gh api /orgs/racwa/teams/$slug/members --paginate --jq '.[].login' | ForEach-Object {
        Write-Host "Removing $_ from $slug"
        gh api --method DELETE /orgs/racwa/teams/$slug/memberships/$_
    }
}
```

To verify all memberships are cleared:

```powershell
gh api /orgs/racwa/teams/copilot-challenge/teams --paginate --jq '.[].slug' | ForEach-Object {
    $count = (gh api /orgs/racwa/teams/$_/members --paginate | ConvertFrom-Json).Count
    Write-Host "$_ : $count members"
}
```

---

## 3. Set the Challenge Start Date

Set to the actual go-live date (ISO 8601 format to avoid locale ambiguity).

```powershell
az webapp config appsettings set `
  --name prd-eyj6v `
  --resource-group rg-github-copilot-challenge-prd `
  --settings 'ChallengeSettings__ChallengeStartDate=YYYY-MM-DD'
```

Replace `YYYY-MM-DD` with the go-live date, e.g. `2026-03-16`.

> Scoring in `ScoringService` filters GitHub Copilot metrics to dates **on or after** this value.

---

## 4. Enable the Challenge

This locks the Profile page (name, GitHub handle, team selection are disabled for all users) and activates scoring.

```powershell
az webapp config appsettings set `
  --name prd-eyj6v `
  --resource-group rg-github-copilot-challenge-prd `
  --settings 'ChallengeSettings__ChallengeStarted=true'
```

> App Service will restart automatically after this change (~30 seconds).

---

## 5. Verify

- [ ] Browse to `https://prd-eyj6v.azurewebsites.net` — app loads without errors
- [ ] Go to `/User/Profile` — all fields and buttons are disabled (challenge has started)
- [ ] Go to `/Home/AllChallenges` — challenge list renders correctly
- [ ] Trigger a scoring run (via the Scoring endpoint or wait for the background job) and verify leaderboard populates
- [ ] Spot-check GitHub team membership via:
  ```powershell
  gh api /orgs/racwa/teams/copilot-challenge/teams --jq '.[].name'
  ```

---

## Rollback

To re-open registration (e.g. if something goes wrong before go-live):

```powershell
az webapp config appsettings set `
  --name prd-eyj6v `
  --resource-group rg-github-copilot-challenge-prd `
  --settings 'ChallengeSettings__ChallengeStarted=false'
```

---

## Reference: All Challenge-Related App Settings

| Setting | Purpose | Example Value |
|---|---|---|
| `ChallengeSettings__ChallengeStarted` | Locks profiles / enables scoring | `true` |
| `ChallengeSettings__ChallengeStartDate` | Earliest date for Copilot metrics scoring | `2026-03-16` |
| `ChallengeSettings__MaxParticipantsPerTeam` | Cap on team size | `8` |
| `GitHubSettings__Enabled` | Enables GitHub API integration | `true` |
| `GitHubSettings__Org` | GitHub org slug | `racwa` |
| `GitHubSettings__PAT` | KV reference to GitHub PAT | `@Microsoft.KeyVault(...)` |
| `GitHubSettings__ParentTeamSlug` | Parent team slug (default: `copilot-challenge`) | `copilot-challenge` |
