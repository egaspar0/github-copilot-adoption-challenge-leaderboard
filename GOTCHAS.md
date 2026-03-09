# Operational Gotchas

Notes on non-obvious issues encountered during setup and operation.

---

## GitHub Copilot Metrics API

### Empty metrics for challenge teams
The `/orgs/{org}/team/{slug}/copilot/metrics` endpoint only returns data for teams that have **Copilot seats directly assigned** to them in the org's Copilot Access settings.

If seats are assigned to a catch-all team (e.g. `github-copilot-users`), challenge teams will return `[]` even if members are actively using Copilot.

**Fix:** Add each challenge team to the org's Copilot Access list:
`https://github.com/organizations/{org}/settings/copilot/policies`

### Metrics are not backfilled
Adding a team to Copilot Access only tracks usage **from that point forward**. Historical usage remains attributed to the team that originally granted the seat. There is no backfill.

There is also typically a **~24 hour delay** before a newly added team starts showing metrics.

### Metrics require specific PAT scopes
The PAT used for `GitHubSettings__PAT` must include the **`manage_billing:copilot`** scope (not just `copilot`). Scope changes take effect immediately on the next API call.

Check current scopes:
```bash
gh api /user -i 2>&1 | Select-String "x-oauth-scopes"
```

### Usage metrics must be enabled in org settings
Even with correct PAT scopes, the API returns `[]` if the org hasn't enabled usage metrics:
`https://github.com/organizations/{org}/settings/copilot/policies`
→ **"Copilot usage metrics for this organization"** must be set to **Enabled**.

### Users in multiple teams = double-counted metrics
The metrics API reports team-level aggregates. A user who belongs to two teams will appear in both teams' `active_users` count. Keep challenge participants in exactly one challenge team.

---

## Configuration

### ChallengeStartDate format
Use ISO 8601 (`yyyy-MM-dd`, e.g. `2025-10-04`) to avoid ambiguity between `MM/dd/yyyy` and `dd/MM/yyyy`. The value is parsed with `CultureInfo.InvariantCulture` — Azure App Service (Linux) defaults to `en-US` so `10/04/2025` would mean **October 4**, not April 10.

### ChallengeStarted vs ChallengeStartDate
- **`ChallengeStarted`** (`true`/`false`) — locks participant profile editing (team, GitHub handle, etc.). Set to `true` when the challenge goes live.
- **`ChallengeStartDate`** (date string) — filters out GitHub Copilot metrics that predate the challenge. Only metrics on or after this date are scored.

---

## Application Insights / Logging

### "Executed DbCommand" flooding traces
EF Core logs all SQL commands at `Information` level by default. Suppress with:
```json
"Logging": {
  "LogLevel": {
    "Microsoft.EntityFrameworkCore.Database.Command": "Warning",
    "Microsoft.EntityFrameworkCore": "Warning"
  }
}
```

### Filtering to app logs only (KQL)
```kusto
traces
| where customDimensions.CategoryName startswith "LeaderboardApp"
| order by timestamp desc
```
