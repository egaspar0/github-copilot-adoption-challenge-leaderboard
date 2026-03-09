using LeaderboardApp.DTOs;
using LeaderboardApp.Services;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.AspNetCore.Authorization;

namespace LeaderboardApp.Controllers
{
    [Authorize]
    public class ChallengeController : Controller
    {
        private readonly ChallengesService _challengesService;

        public ChallengeController(ChallengesService challengesService)
        {
            _challengesService = challengesService;
        }

        public async Task<IActionResult> Details(int challengeId)
        {
            var challenge = await _challengesService.GetChallengeByIdAsync(challengeId);
            if (challenge == null || challenge.ActivityId != 12)
            {
                return NotFound();
            }

            return View(challenge);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLink(int challengeId)
        {
            var challenge = await _challengesService.GetChallengeByIdAsync(challengeId);
            if (challenge == null || challenge.ActivityId != 13)
            {
                return NotFound();
            }

            return View(challenge);
        }

        [HttpPost]
        public async Task<IActionResult> LinkClicked([FromBody] ChallengeDto input)
        {
            var challenge = await _challengesService.GetChallengeByIdAsync(input.ChallengeId);
            if (challenge == null || challenge.ActivityId != 13)
            {
                return BadRequest("Invalid challenge.");
            }

            try
            {
                return await CompleteChallenge(input.ChallengeId);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SubmitChallengeAjax([FromBody] SubmitChallengeDto input)
        {
            var challenge = await _challengesService.GetChallengeByIdAsync(input.ChallengeId);
            if (challenge == null || challenge.ActivityId != 12)
            {
                return BadRequest("Invalid challenge.");
            }

            try
            {
                // Store survey responses if any
                if (input.SurveyResponses != null && input.SurveyResponses.Count > 0)
                {
                    // Serialize survey responses to JSON string
                    var surveyResponsesJson = System.Text.Json.JsonSerializer.Serialize(input.SurveyResponses);
                    return await CompleteChallenge(input.ChallengeId, surveyResponsesJson);
                }
                else
                {
                    // Original behavior when there are no survey responses
                    return await CompleteChallenge(input.ChallengeId);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        private async Task<IActionResult> CompleteChallenge(int challengeId, string? validationData = null)
        {
            // Check if the challenge is already completed
            //Get current user Id
            var userId = await _challengesService.GetUserIdAsync();
            if (userId == null)
                return Unauthorized("User not authenticated.");

            var isAlreadyCompleted = await _challengesService.IsChallengeAlreadyCompleted(userId.Value, challengeId);
            if (isAlreadyCompleted)
                return BadRequest("Challenge already completed.");

            // Mark the challenge as completed
            var success = await _challengesService.MarkChallengeCompleted(challengeId, validationData);
            if (!success)
                return StatusCode(500, "Failed to mark the challenge as completed.");

            // Optionally update the leaderboard
            var teamId = await _challengesService.GetUserTeamIdAsync(userId.Value);
            if (teamId.HasValue)
            {
                await _challengesService.UpdateLeaderboardAsync(teamId.Value);
            }
            return Ok("Challenge successfully completed.");
        }
    }

    public class SubmitChallengeDto
    {
        public int ChallengeId { get; set; }
        public List<QuestionResponseDto> SurveyResponses { get; set; } = new List<QuestionResponseDto>();
    }

    public class QuestionResponseDto
    {
        public string QuestionText { get; set; }
        public string SelectedOption { get; set; }
    }
}