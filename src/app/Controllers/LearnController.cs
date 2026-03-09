using LeaderboardApp.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.Json;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;

namespace LeaderboardApp.Controllers
{
    [Authorize]
    public class LearnController : Controller
    {
        private readonly ChallengesService _challengesService;
        private readonly LearnService _learnService;    

        public LearnController(ChallengesService challengesService, LearnService learnService)
        {
            _challengesService = challengesService;
            _learnService = learnService;
        }

        public async Task<IActionResult> Module(int challengeId)
        {
            var challenge = await _challengesService.GetChallengeByIdAsync(challengeId);
            if (challenge == null || challenge.ActivityId != 10)
            {
                return NotFound();
            }

            return View(challenge);
        }

        public async Task<IActionResult> Certification(int challengeId)
        {
            var challenge = await _challengesService.GetChallengeByIdAsync(challengeId);
            if (challenge == null || challenge.ActivityId != 11)
            {
                return NotFound();
            }

            return View(challenge);
        }

        [HttpPost]
        public IActionResult SubmitValidationLinkNew([FromBody] ValidationSubmission submission)
        {
            if (submission == null || string.IsNullOrEmpty(submission.ValidationLink))
                return BadRequest("Missing submission data.");

            // You can add regex validation here
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitCertification([FromBody] ValidationSubmission submission)
        {
            if(string.IsNullOrWhiteSpace(submission.ValidationLink))
            {
                return BadRequest("Missing or invalid submission data.");
            }
            await _challengesService.MarkChallengeCompleted(submission.ChallengeId, submission.ValidationLink);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitValidationLink([FromBody] ValidationSubmission submission)
        {
            var msLearnHandle = await _learnService.GetCurrentUserLearnHandleAsync(User);

            if (submission == null || string.IsNullOrWhiteSpace(submission.ValidationLink))
                return BadRequest("Missing or invalid submission data.");

            if (!Uri.TryCreate(submission.ValidationLink, UriKind.Absolute, out Uri? uri) ||
                !(uri.Host.Contains("learn.microsoft.com")))
            {
                return BadRequest("The provided link does not appear to be a valid Learn achievement URL.");
            }
            
            // Validate URL pattern with regex to ensure it's an achievements URL regardless of locale
            var achievementUrlPattern = new Regex(@"^https:\/\/learn\.microsoft\.com\/(?:[a-z]{2}-[a-z]{2}\/)?users\/[^\/]+\/achievements\/[a-z0-9]+$", RegexOptions.IgnoreCase);
            if (!achievementUrlPattern.IsMatch(submission.ValidationLink))
            {
                return BadRequest("The provided link does not appear to be a valid Learn achievement URL.");
            }

            var challenge = await _challengesService.GetChallengeByIdAsync(submission.ChallengeId);
            if (challenge == null)
                return BadRequest("Invalid challenge reference.");
            
            if (string.IsNullOrWhiteSpace(msLearnHandle))
                return BadRequest("MS Learn handle is missing for this challenge.");

            try
            {
                using var httpClient = new HttpClient();

                // Step 1: Get Profile Info
                var profileApiUrl = $"https://learn.microsoft.com/api/profiles/{msLearnHandle}";
                var profileJson = await httpClient.GetStringAsync(profileApiUrl);
                var profile = JsonSerializer.Deserialize<Profile>(profileJson);

                if (profile == null || string.IsNullOrWhiteSpace(profile.userId))
                    return BadRequest("Could not retrieve profile information from Learn API.");

                // Step 2: Get Achievement Info
                // Extract achievement ID regardless of locale in the URL
                var pathSegments = uri.AbsolutePath.Split('/');
                var achievementId = pathSegments[pathSegments.Length - 1];
                var achievementApiUrl = $"https://learn.microsoft.com/api/achievements/{achievementId}";
                var achievementJson = await httpClient.GetStringAsync(achievementApiUrl);
                var achievement = JsonSerializer.Deserialize<Achievement>(achievementJson);

                if (achievement == null || string.IsNullOrWhiteSpace(achievement.title))
                    return BadRequest("Could not retrieve achievement details.");

                // Step 3: Cross-check userId matches
                if (!string.Equals(profile.userId, achievement.userId, StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest("This achievement does not belong to the expected user.");
                }

                // Step 4: Compare titles
                string Normalize(string s) =>
                    new string(s.ToLowerInvariant().Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)).ToArray());

                var expected = Normalize(challenge.Title);
                var actual = Normalize(achievement.title);

                if (!string.Equals(actual, expected, StringComparison.Ordinal))
                    return BadRequest($"Module mismatch. Found '{achievement.title}', expected '{challenge.Title}'.");  

                // Optional: check grantedOn or verified = true if needed

                await _challengesService.MarkChallengeCompleted(submission.ChallengeId, submission.ValidationLink);
                return Ok();
            }
            catch (HttpRequestException ex)
            {
                return BadRequest("Error connecting to Learn API: " + ex.Message);
            }
            catch (JsonException ex)
            {
                return BadRequest("Error parsing Learn API response: " + ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred: " + ex.Message);
            }
        }
    }

    public class ValidationSubmission
    {
        public int ChallengeId { get; set; }
        public string ValidationLink { get; set; }
    }

    public class Achievement
    {
        public string id { get; set; }
        public string title { get; set; }
        public string userId { get; set; }
        public string grantedOn { get; set; }
        public string typeId { get; set; }
        public bool verified { get; set; }
        public string url { get; set; }
    }

    public class Profile
    {
        public string userId { get; set; }
        public string userName { get; set; }
        public string displayName { get; set; }
        public bool isPrivate { get; set; }
        public string[] affiliations { get; set; }
    }
}