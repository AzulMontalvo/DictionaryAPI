using DictionaryAPI.Interfaces;
using DictionaryAPI.Models.DTOs.Term;
using DictionaryAPI.Models.Entities;
using DictionaryAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DictionaryAPI.Controllers
{
    [Route("api/terms")]
    [ApiController]
    public class TermsController : ControllerBase
    {
        private readonly ITermService _termService;

        public TermsController(ITermService termService)
        {
            _termService = termService;
        }

        #region "PUBLIC"
        //[HttpGet]
        //public async Task<IActionResult> GetAllPublicTerms()
        //    => Ok(await _termService.GetAllPublicTermsAsync());
        [HttpGet("daily-word")]
        public async Task<IActionResult> GetDailyWord()
        {
            var dailyWord = await _termService.GetDailyWordAsync();

            if (dailyWord is null)
                return NotFound();

            return Ok(dailyWord);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPublicTermById(int id)
        {
            var term = await _termService.GetPublicTermByIdAsync(id);
            if (term == null)
            {
                return NotFound(new { Message = "Term not found." });
            }
            return Ok(term);
        }

        [HttpGet("etymology/{language}")]
        public async Task<IActionResult> GetByEtymology(string language)
        {
            var terms = await _termService.GetTermsByEtymologyAsync(language);
            return Ok(terms);
        }
        #endregion

        #region "SHARED"
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetTerms(
            [FromQuery] string? query,
            [FromQuery] string? category,
            [FromQuery] bool? isVisible,
            [FromQuery] bool? hasVideo,
            [FromQuery] string? orderBy)
        {
            TermCategory? mappedCategory = null;
            if (!string.IsNullOrWhiteSpace(category) && Enum.TryParse<TermCategory>(category, true, out var parsedCategory))
            {
                mappedCategory = parsedCategory;
            }

            if (User.IsInRole("Admin"))
            {
                var adminResults = await _termService.SearchPrivateTermsAsync(query, mappedCategory, isVisible, hasVideo, orderBy);
                return Ok(adminResults);
            }

            var results = await _termService.SearchPublicTermsAsync(query, mappedCategory, orderBy);
            return Ok(results);
        }
        #endregion
    }
}
