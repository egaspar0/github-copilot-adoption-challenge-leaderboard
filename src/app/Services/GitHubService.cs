using LeaderboardApp.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LeaderboardApp.Services
{
    public class GitHubService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GitHubService> _logger;
        private static readonly string GHApiURLPrefix = "https://api.github.com/orgs";
        private readonly bool _enabled;

        public GitHubService(HttpClient httpClient, IConfiguration configuration, ILogger<GitHubService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _enabled = configuration.GetValue<bool>("GitHubSettings:Enabled", true);

            if (!_enabled)
            {
                _logger.LogWarning("GitHub integration disabled via configuration (GitHubSettings:Enabled=false). All GitHubService calls will be no-ops.");
                return; // Do not configure client
            }

            // Configure default headers only once
            if (!_httpClient.DefaultRequestHeaders.Contains("Authorization"))
            {
                var token = _configuration["GitHubSettings:PAT"];
                if (!string.IsNullOrWhiteSpace(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }

            if (!_httpClient.DefaultRequestHeaders.Accept.Any(h => h.MediaType == "application/vnd.github+json"))
            {
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            }

            if (!_httpClient.DefaultRequestHeaders.Contains("X-GitHub-Api-Version"))
            {
                _httpClient.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
            }

            if (!_httpClient.DefaultRequestHeaders.UserAgent.Any())
            {
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("LeaderboardApp");
            }
        }

        private bool IsDisabled()
        {
            if (_enabled) return false;
            _logger.LogDebug("GitHubService call skipped because integration is disabled.");
            return true;
        }

        public async Task<List<GitHubMetrics>?> GetOrgCopilotMetricsAsync()
        {
            if (IsDisabled()) return new List<GitHubMetrics>();

            var org = _configuration["GitHubSettings:Org"];
            if (string.IsNullOrWhiteSpace(org))
            {
                _logger.LogWarning("GitHub organization is not configured.");
                return null;
            }

            var url = $"{GHApiURLPrefix}/{org}/copilot/metrics";
            try
            {
                var response = await _httpClient.GetAsync(url);
                var jsonResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to fetch organization Copilot metrics. Status: {StatusCode}, Response: {Response}", response.StatusCode, jsonResponse);
                    return null;
                }

                var metrics = JsonSerializer.Deserialize<List<GitHubMetrics>>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Successfully fetched organization Copilot metrics.");
                return metrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching organization Copilot metrics.");
                return null;
            }
        }

        public async Task<string> CreateTeamAsync(string teamName)
        {
            if (IsDisabled()) return string.Empty;

            var org = _configuration["GitHubSettings:Org"];
            if (string.IsNullOrWhiteSpace(org))
            {
                _logger.LogWarning("GitHub organization is not configured.");
                return string.Empty;
            }

            int? parentTeamId = await GetOrCreateParentTeamIdAsync(org);

            var githubPayload = parentTeamId.HasValue
                ? (object)new
                {
                    name = teamName,
                    description = "Auto-created from Copilot Challenge app",
                    permission = "push",
                    notification_setting = "notifications_enabled",
                    privacy = "closed",
                    parent_team_id = parentTeamId.Value
                }
                : new
                {
                    name = teamName,
                    description = "Auto-created from Copilot Challenge app",
                    permission = "push",
                    notification_setting = "notifications_enabled",
                    privacy = "closed"
                };

            var response = await _httpClient.PostAsync(
                $"{GHApiURLPrefix}/{org}/teams",
                new StringContent(JsonSerializer.Serialize(githubPayload), Encoding.UTF8, "application/json")
            );

            var jsonResponse = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("GitHub team creation failed. Status: {StatusCode}, Response: {Response}",
                    response.StatusCode, jsonResponse);                
                return string.Empty;
            }

            try
            {
                using var doc = JsonDocument.Parse(jsonResponse);
                var slug = doc.RootElement.GetProperty("slug").GetString();
                _logger.LogInformation("GitHub team created successfully. Slug: {Slug}", slug);

                // Remove any users in the newly created team
                await RemoveAllUsersFromTeamAsync(slug);
                _logger.LogInformation("Removed all users from newly created team {Slug}", slug);

                return slug ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse slug from GitHub response: {Response}", jsonResponse);
                return string.Empty;
            }
        }

        private async Task<int?> GetOrCreateParentTeamIdAsync(string org)
        {
            var parentSlug = _configuration["GitHubSettings:ParentTeamSlug"];
            if (string.IsNullOrWhiteSpace(parentSlug))
            {
                parentSlug = "copilot-challenge";
            }

            // Try to get the existing parent team
            var getResponse = await _httpClient.GetAsync($"{GHApiURLPrefix}/{org}/teams/{parentSlug}");
            if (getResponse.IsSuccessStatusCode)
            {
                try
                {
                    var json = await getResponse.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var id = doc.RootElement.GetProperty("id").GetInt32();
                    _logger.LogInformation("Found existing parent team '{Slug}' with id {Id}.", parentSlug, id);
                    return id;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to parse parent team response for slug '{Slug}'.", parentSlug);
                    return null;
                }
            }

            if (getResponse.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                var errorBody = await getResponse.Content.ReadAsStringAsync();
                _logger.LogError("Unexpected response when fetching parent team '{Slug}'. Status: {Status}, Body: {Body}",
                    parentSlug, getResponse.StatusCode, errorBody);
                return null;
            }

            // Parent team does not exist — create it
            _logger.LogInformation("Parent team '{Slug}' not found. Creating it.", parentSlug);
            var createPayload = new
            {
                name = "Copilot Challenge",
                description = "Parent team for all Copilot Challenge teams",
                privacy = "closed"
            };

            var createResponse = await _httpClient.PostAsync(
                $"{GHApiURLPrefix}/{org}/teams",
                new StringContent(JsonSerializer.Serialize(createPayload), Encoding.UTF8, "application/json")
            );

            var createJson = await createResponse.Content.ReadAsStringAsync();
            if (!createResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to create parent team. Status: {Status}, Body: {Body}",
                    createResponse.StatusCode, createJson);
                return null;
            }

            try
            {
                using var doc = JsonDocument.Parse(createJson);
                var id = doc.RootElement.GetProperty("id").GetInt32();
                _logger.LogInformation("Created parent team 'Copilot Challenge' with id {Id}.", id);
                return id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse created parent team response.");
                return null;
            }
        }


        private string GetParentTeamSlug() =>
            string.IsNullOrWhiteSpace(_configuration["GitHubSettings:ParentTeamSlug"])
                ? "copilot-challenge"
                : _configuration["GitHubSettings:ParentTeamSlug"]!;

        private async Task<bool> IsTeamInScopeAsync(string org, string teamSlug)
        {
            var parentSlug = GetParentTeamSlug();

            // The parent team itself is always in scope
            if (teamSlug == parentSlug) return true;

            try
            {
                var response = await _httpClient.GetAsync($"{GHApiURLPrefix}/{org}/teams/{teamSlug}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("IsTeamInScopeAsync: could not retrieve team '{TeamSlug}'. Status: {Status}", teamSlug, response.StatusCode);
                    return false;
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("parent", out var parentEl) &&
                    parentEl.ValueKind != JsonValueKind.Null &&
                    parentEl.TryGetProperty("slug", out var slugEl))
                {
                    var actualParent = slugEl.GetString();
                    if (actualParent == parentSlug) return true;

                    _logger.LogWarning("Team '{TeamSlug}' has parent '{ActualParent}', not the configured parent '{ExpectedParent}'. Operation blocked.",
                        teamSlug, actualParent, parentSlug);
                    return false;
                }

                _logger.LogWarning("Team '{TeamSlug}' has no parent team. Operation blocked to prevent acting outside Copilot Challenge scope.", teamSlug);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to verify team scope for slug '{TeamSlug}'. Operation blocked.", teamSlug);
                return false;
            }
        }

        public async Task<string?> GetLastUserActivity(string? githubHandle)
        {
            if (IsDisabled()) return null;

            if (string.IsNullOrWhiteSpace(githubHandle))
            {
                _logger.LogWarning("GitHub handle is null or empty.");
                return null;
            }

            var org = _configuration["GitHubSettings:Org"];
            if (string.IsNullOrWhiteSpace(org))
            {
                _logger.LogWarning("GitHub organization is not configured.");
                return null;
            }

            try
            {
                var url = $"{GHApiURLPrefix}/{org}/members/{githubHandle}/copilot";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to fetch last user activity. Status: {StatusCode}, Response: {Response}",
                        response.StatusCode, errorResponse);
                    return null;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var activityData = JsonSerializer.Deserialize<GitHubActivityResponse>(jsonResponse);

                if (activityData?.LastActivityAt != null)
                {
                    _logger.LogInformation("Last activity for user {GitHubHandle}: {LastActivityAt}", githubHandle, activityData.LastActivityAt);
                    return activityData.LastActivityAt;
                }

                _logger.LogWarning("No last activity found for user {GitHubHandle}.", githubHandle);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching last user activity for {GitHubHandle}.", githubHandle);
                return null;
            }
        }

        public async Task<bool> MoveUserToTeamAsync(string githubUsername, string? oldTeamSlug, string newTeamSlug)
        {
            if (IsDisabled()) return true; // treat as success to not block flows

            var org = _configuration["GitHubSettings:Org"];
            if (string.IsNullOrWhiteSpace(org)) { _logger.LogWarning("GitHub organization is not configured."); return false; }

            if (!await IsTeamInScopeAsync(org, newTeamSlug))
            {
                _logger.LogError("MoveUserToTeamAsync blocked: target team '{TeamSlug}' is not within Copilot Challenge scope.", newTeamSlug);
                return false;
            }

            if (!string.IsNullOrWhiteSpace(oldTeamSlug) && !await IsTeamInScopeAsync(org, oldTeamSlug))
            {
                _logger.LogError("MoveUserToTeamAsync blocked: source team '{TeamSlug}' is not within Copilot Challenge scope.", oldTeamSlug);
                return false;
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(oldTeamSlug))
                {
                    var removeUrl = $"{GHApiURLPrefix}/{org}/teams/{oldTeamSlug}/memberships/{githubUsername}";
                    _logger.LogInformation("Removing user {GitHubUsername} from team {TeamSlug} via URL: {Url}",
                        githubUsername, oldTeamSlug, removeUrl);

                    var removeResponse = await _httpClient.DeleteAsync(removeUrl);

                    if (!removeResponse.IsSuccessStatusCode)
                    {
                        var errorContent = await removeResponse.Content.ReadAsStringAsync();
                        _logger.LogWarning("Failed to remove user {GitHubUsername} from old team {OldTeamSlug}. Status: {StatusCode}, Response: {Response}",
                            githubUsername, oldTeamSlug, removeResponse.StatusCode, errorContent);
                    }
                    else
                    {
                        _logger.LogInformation("Successfully removed user {GitHubUsername} from old team {TeamSlug}",
                            githubUsername, oldTeamSlug);
                    }
                }

                var addUrl = $"{GHApiURLPrefix}/{org}/teams/{newTeamSlug}/memberships/{githubUsername}";
                _logger.LogInformation("Adding user {GitHubUsername} to team {TeamSlug} via URL: {Url}",
                    githubUsername, newTeamSlug, addUrl);

                var addResponse = await _httpClient.PutAsync(addUrl, null);

                if (!addResponse.IsSuccessStatusCode)
                {
                    var errorResponse = await addResponse.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to add user {GitHubUsername} to new team {NewTeamSlug}. Status: {StatusCode}, Response: {Response}",
                        githubUsername, newTeamSlug, addResponse.StatusCode, errorResponse);
                    return false;
                }

                _logger.LogInformation("Successfully moved user {GitHubUsername} to team {NewTeamSlug}.", githubUsername, newTeamSlug);

                // Remove direct parent team membership if configured — child team membership implies parent via inheritance
                var parentSlug = GetParentTeamSlug();
                if (!string.IsNullOrWhiteSpace(parentSlug) && newTeamSlug != parentSlug)
                {
                    var removeParentUrl = $"{GHApiURLPrefix}/{org}/teams/{parentSlug}/memberships/{githubUsername}";
                    var removeParentResponse = await _httpClient.DeleteAsync(removeParentUrl);
                    if (removeParentResponse.IsSuccessStatusCode || removeParentResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        _logger.LogInformation("Removed direct parent team membership for {GitHubUsername} from '{ParentSlug}'.", githubUsername, parentSlug);
                    }
                    else
                    {
                        _logger.LogWarning("Could not remove direct parent membership for {GitHubUsername} from '{ParentSlug}'. Status: {Status}",
                            githubUsername, parentSlug, removeParentResponse.StatusCode);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in MoveUserToTeamAsync for user {GitHubUsername} to team {TeamSlug}",
                    githubUsername, newTeamSlug);
                return false;
            }
        }

        public async Task<bool> RemoveAllUsersFromTeamAsync(string teamSlug)
        {
            if (IsDisabled()) return true; // nothing to do

            var org = _configuration["GitHubSettings:Org"];
            if (string.IsNullOrWhiteSpace(org)) { _logger.LogWarning("GitHub organization is not configured."); return false; }

            if (!await IsTeamInScopeAsync(org, teamSlug))
            {
                _logger.LogError("RemoveAllUsersFromTeamAsync blocked: team '{TeamSlug}' is not within Copilot Challenge scope.", teamSlug);
                return false;
            }

            try
            {
                var membersUrl = $"{GHApiURLPrefix}/{org}/teams/{teamSlug}/members";
                var response = await _httpClient.GetAsync(membersUrl);

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to fetch team members for team {TeamSlug}. Status: {StatusCode}, Response: {Response}",
                        teamSlug, response.StatusCode, errorResponse);
                    return false;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var members = JsonSerializer.Deserialize<List<GitHubMember>>(jsonResponse);

                if (members == null || !members.Any()) { _logger.LogInformation("No members found in team {TeamSlug}.", teamSlug); return true; }

                foreach (var member in members)
                {
                    var removeUrl = $"{GHApiURLPrefix}/{org}/teams/{teamSlug}/memberships/{member.Login}";
                    var removeResponse = await _httpClient.DeleteAsync(removeUrl);

                    if (!removeResponse.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Failed to remove user {GitHubUsername} from team {TeamSlug}. Status: {StatusCode}",
                            member.Login, teamSlug, removeResponse.StatusCode);
                    }
                    else
                    {
                        _logger.LogInformation("Successfully removed user {GitHubUsername} from team {TeamSlug}.", member.Login, teamSlug);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while removing all users from team {TeamSlug}.", teamSlug);
                return false;
            }
        }

        public async Task<bool> RemoveUserFromTeamAsync(string teamSlug, string githubUsername)
        {
            if (IsDisabled()) return true;

            var org = _configuration["GitHubSettings:Org"];
            if (string.IsNullOrWhiteSpace(org)) { _logger.LogWarning("GitHub organization is not configured."); return false; }

            if (!await IsTeamInScopeAsync(org, teamSlug))
            {
                _logger.LogError("RemoveUserFromTeamAsync blocked: team '{TeamSlug}' is not within Copilot Challenge scope.", teamSlug);
                return false;
            }

            try
            {
                var removeUrl = $"{GHApiURLPrefix}/{org}/teams/{teamSlug}/memberships/{githubUsername}";
                _logger.LogInformation("Removing user {GitHubUsername} from team {TeamSlug} via URL: {Url}",
                    githubUsername, teamSlug, removeUrl);

                var removeResponse = await _httpClient.DeleteAsync(removeUrl);

                if (!removeResponse.IsSuccessStatusCode)
                {
                    var errorContent = await removeResponse.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to remove user {GitHubUsername} from team {TeamSlug}. Status: {StatusCode}, Response: {Response}",
                        githubUsername, teamSlug, removeResponse.StatusCode, errorContent);
                    return false;
                }
                else
                {
                    _logger.LogInformation("Successfully removed user {GitHubUsername} from team {TeamSlug}",
                        githubUsername, teamSlug);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in RemoveUserFromTeamAsync for user {GitHubUsername} from team {TeamSlug}",
                    githubUsername, teamSlug);
                return false;
            }
        }

        public async Task<List<GitHubMetrics>?> GetCopilotMetricsAsync(string teamSlug)
        {
            if (IsDisabled()) return new List<GitHubMetrics>();

            var org = _configuration["GitHubSettings:Org"];
            if (string.IsNullOrWhiteSpace(org)) { _logger.LogWarning("GitHub organization is not configured."); return null; }
            var url = $"{GHApiURLPrefix}/{org}/team/{teamSlug}/copilot/metrics";
            try
            {
                var response = await _httpClient.GetAsync(url);
                var jsonResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to fetch Copilot metrics. Status: {StatusCode}, Response: {Response}", response.StatusCode, jsonResponse);
                    return null;
                }

                var metrics = JsonSerializer.Deserialize<List<GitHubMetrics>>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return metrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching GitHub Copilot metrics.");
                return null;
            }
        }

        public async Task<List<GitHubMember>?> GetTeamMembersAsync(string? teamSlug)
        {
            if (IsDisabled()) return new List<GitHubMember>();

            if (string.IsNullOrWhiteSpace(teamSlug)) { _logger.LogWarning("Team slug is null or empty."); return null; }
            var org = _configuration["GitHubSettings:Org"];
            if (string.IsNullOrWhiteSpace(org)) { _logger.LogWarning("GitHub organization is not configured."); return null; }
            try
            {
                var membersUrl = $"{GHApiURLPrefix}/{org}/teams/{teamSlug}/members";
                _logger.LogInformation("Fetching team members from URL: {Url}", membersUrl);
                var response = await _httpClient.GetAsync(membersUrl);
                var jsonResponse = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to fetch team members for team {TeamSlug}. Status: {StatusCode}, Response: {Response}", teamSlug, response.StatusCode, jsonResponse);
                    return null;
                }
                var members = JsonSerializer.Deserialize<List<GitHubMember>>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                _logger.LogInformation("Successfully retrieved {Count} members for team {TeamSlug}", members?.Count ?? 0, teamSlug);
                return members;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching members for team {TeamSlug}.", teamSlug);
                return null;
            }
        }

        public async Task<bool> DeleteTeamAsync(string teamSlug)
        {
            if (IsDisabled()) return true;

            var org = _configuration["GitHubSettings:Org"];
            if (string.IsNullOrWhiteSpace(org)) { _logger.LogWarning("GitHub organization is not configured."); return false; }

            try
            {
                var url = $"{GHApiURLPrefix}/{org}/teams/{teamSlug}";
                var response = await _httpClient.DeleteAsync(url);

                // 204 = deleted, 404 = already gone - both are fine
                if (!response.IsSuccessStatusCode &&
                    response.StatusCode != System.Net.HttpStatusCode.NotFound)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError(
                        "Failed to delete GitHub team '{TeamSlug}'. Status: {StatusCode}, Response: {Response}",
                        teamSlug, response.StatusCode, errorContent);
                    return false;
                }

                _logger.LogInformation("Successfully deleted GitHub team '{TeamSlug}'", teamSlug);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception deleting GitHub team '{TeamSlug}'", teamSlug);
                return false;
            }
        }

        public async Task<List<GitHubTeam>?> GetAllTeamsAsync()
        {
            if (IsDisabled()) return new List<GitHubTeam>();

            var org = _configuration["GitHubSettings:Org"];
            if (string.IsNullOrWhiteSpace(org)) { _logger.LogWarning("GitHub organization is not configured."); return null; }

            try
            {
                var url = $"{GHApiURLPrefix}/{org}/teams?per_page=100";
                var response = await _httpClient.GetAsync(url);
                var jsonResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "Failed to list GitHub teams. Status: {StatusCode}, Response: {Response}",
                        response.StatusCode, jsonResponse);
                    return null;
                }

                var teams = JsonSerializer.Deserialize<List<GitHubTeam>>(jsonResponse,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                _logger.LogInformation("Retrieved {Count} GitHub teams for org '{Org}'", teams?.Count ?? 0, org);
                return teams;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching GitHub teams for org '{Org}'", org);
                return null;
            }
        }

        /// <summary>
        /// Attempts to resolve the GitHub login for a user by querying the SCIM external identities API.
        /// Tries Entra Object ID (<paramref name="entraOid"/>) first, then UPN (<paramref name="upn"/>) as fallback.
        /// Uses the GitHub GraphQL <c>externalIdentities</c> API (GHEC + SAML) to resolve the
        /// GitHub login for an Entra-authenticated user, filtering by <c>userName</c> (the SAML NameID,
        /// which Azure AD sends as the user's UPN / email address).
        /// Returns <c>null</c> when no match is found or GitHub integration is disabled.
        /// </summary>
        public async Task<string?> LookupGitHubLoginByScimAsync(string? entraOid, string? upn)
        {
            if (IsDisabled()) return null;

            var org = _configuration["GitHubSettings:Org"];
            if (string.IsNullOrWhiteSpace(org))
            {
                _logger.LogWarning("GitHub handle lookup skipped: GitHub organisation is not configured.");
                return null;
            }

            // GitHub's externalIdentities GraphQL connection only supports `login` and `userName` filters.
            // For Azure AD SAML, the NameID is the UPN (email), so we use that as userName.
            if (string.IsNullOrWhiteSpace(upn))
            {
                _logger.LogWarning("GitHub handle lookup skipped: UPN/email claim is null or empty.");
                return null;
            }

            var login = await TryGraphQLExternalIdentityAsync(org, upn);
            if (!string.IsNullOrEmpty(login))
            {
                _logger.LogInformation(
                    "Resolved GitHub login '{Login}' via GraphQL externalIdentities for UPN={Upn}.", login, upn);
                return login;
            }

            _logger.LogInformation(
                "GitHub handle lookup: no login resolved for UPN={Upn}. " +
                "The user may not have authorised their GitHub account via SAML SSO yet.",
                upn);
            return null;
        }

        /// <summary>
        /// Queries <c>organization.samlIdentityProvider.externalIdentities(userName: ...)</c> via GraphQL
        /// and returns the linked GitHub user's login, or <c>null</c> if not found / not yet linked.
        /// </summary>
        private async Task<string?> TryGraphQLExternalIdentityAsync(string org, string userName)
        {
            const string query = """
                query ($org: String!, $userName: String!) {
                  organization(login: $org) {
                    samlIdentityProvider {
                      externalIdentities(first: 1, userName: $userName) {
                        nodes {
                          user {
                            login
                          }
                        }
                      }
                    }
                  }
                }
                """;

            var payload = JsonSerializer.Serialize(new
            {
                query,
                variables = new { org, userName }
            });

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.github.com/graphql")
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/json")
                };

                var response = await _httpClient.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "GraphQL externalIdentities (userName={UserName}) returned HTTP {Status}. Body: {Body}",
                        userName, response.StatusCode, json);
                    return null;
                }

                using var doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("errors", out var errorsEl) &&
                    errorsEl.GetArrayLength() > 0)
                {
                    _logger.LogWarning(
                        "GraphQL externalIdentities (userName={UserName}) returned errors: {Errors}",
                        userName, errorsEl.ToString());
                    return null;
                }

                // data → organization → samlIdentityProvider → externalIdentities → nodes[0] → user → login
                if (!doc.RootElement.TryGetProperty("data", out var data)) return null;
                if (!data.TryGetProperty("organization", out var orgEl) ||
                    orgEl.ValueKind == JsonValueKind.Null) return null;
                if (!orgEl.TryGetProperty("samlIdentityProvider", out var idpEl) ||
                    idpEl.ValueKind == JsonValueKind.Null) return null;
                if (!idpEl.TryGetProperty("externalIdentities", out var extIds)) return null;
                if (!extIds.TryGetProperty("nodes", out var nodes) ||
                    nodes.GetArrayLength() == 0) return null;

                var node = nodes[0];
                if (!node.TryGetProperty("user", out var userEl) ||
                    userEl.ValueKind == JsonValueKind.Null)
                {
                    _logger.LogDebug(
                        "GraphQL: SAML identity found for userName={UserName} but user is null " +
                        "(they have not authorised their GitHub account via SAML SSO).", userName);
                    return null;
                }

                if (!userEl.TryGetProperty("login", out var loginEl)) return null;

                return loginEl.GetString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "GraphQL externalIdentities query failed (userName={UserName}).", userName);
                return null;
            }
        }

        /// <summary>
        /// Returns <c>true</c> when <paramref name="githubLogin"/> is a confirmed member of the configured org (HTTP 204).
        /// Always returns <c>true</c> when GitHub integration is disabled so that callers are not blocked in dev/test.
        /// </summary>
        public async Task<bool> IsOrgMemberAsync(string githubLogin)
        {
            if (IsDisabled()) return true; // treat as valid to avoid blocking flows when integration is off

            var org = _configuration["GitHubSettings:Org"];
            if (string.IsNullOrWhiteSpace(org)) return false;

            try
            {
                var response = await _httpClient.GetAsync($"{GHApiURLPrefix}/{org}/members/{githubLogin}");
                // 204 = confirmed member; 302/404 = pending invite or not a member
                return response.StatusCode == System.Net.HttpStatusCode.NoContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking org membership for '{Login}'.", githubLogin);
                return false;
            }
        }

        // Placeholder for GitHubActivityResponse used above (if not already defined elsewhere)
        private class GitHubActivityResponse
        {
            [JsonPropertyName("last_activity_at")] public string? LastActivityAt { get; set; }
        }

        public class GitHubMember { [JsonPropertyName("login")] public string Login { get; set; } = string.Empty; }

        public class GitHubTeam
        {
            [JsonPropertyName("id")] public int Id { get; set; }
            [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
            [JsonPropertyName("slug")] public string Slug { get; set; } = string.Empty;
        }
    }
}