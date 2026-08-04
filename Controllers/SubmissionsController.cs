using DictionaryAPI.Interfaces;
using DictionaryAPI.Models.DTOs.Submission;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DictionaryAPI.Controllers
{
    [ApiController]
    [Route("api/submissions")]
    [Authorize]
    public class SubmissionsController : ControllerBase
    {
        private readonly ISubmissionService _submissionService;

        public SubmissionsController(ISubmissionService submissionService)
        {
            _submissionService = submissionService;
        }

        #region "USER"
        [HttpGet("my")]
        public async Task<IActionResult> GetUserSubmissions()
        {
            var list = await _submissionService.GetUserSubmissionsAsync();
            return Ok(list);
        }
        #endregion

        #region "ADMIN"

        // Admin endpoints
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("/api/admin/submissions")]
        public async Task<IActionResult> GetAdminList([FromQuery] int? status)
        {
            var list = await _submissionService.GetSubmissionsAsync(status);
            return Ok(list);
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        [Route("/api/admin/submissions/{id:int}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            var newTermId = await _submissionService.ApproveSubmissionAsync(id);

            if (newTermId == null)
            {
                return BadRequest(new { Message = "Submission could not be approved. Verify that it exists and is pending." });
            }

            return Ok(new
            {
                Message = "Submission approved successfully. Term has been cloned covertly.",
                TargetTermId = newTermId
            });
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        [Route("/api/admin/submissions/{id:int}/reject")]
        public async Task<IActionResult> Reject(int id)
        {
            var resultado = await _submissionService.RejectSubmissionAsync(id);

            if (!resultado)
            {
                return BadRequest(new { Message = "Submission could not be rejected. Verify that it exists and is pending." });
            }

            return Ok(new { Message = "Submission has been successfully rejected." });
            }
        #endregion

        #region "SHARED"
        [HttpPost]
        public async Task<IActionResult> CreateSubmission([FromBody] NewSubmissionDto request)
        {
            var submission = await _submissionService.CreateSubmissionAsync(request);
            return CreatedAtAction(nameof(GetSubmissionById), new { id = submission.Id }, submission);
        }
        
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetSubmissionById(int id)
        {
            var submission = await _submissionService.GetSubmissionByIdAsync(id);
            if (submission == null) return NotFound(new { Message = "Submission not found" });

            return Ok(submission);
        }
        #endregion
    }
}
