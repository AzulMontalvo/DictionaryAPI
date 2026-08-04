using DictionaryAPI.Interfaces;
using DictionaryAPI.Models.DTOs.Term;
using DictionaryAPI.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DictionaryAPI.Controllers
{
    [Route("api/admin/terms")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminTermsController : ControllerBase
    {
        private readonly ITermService _termService;

        public AdminTermsController(ITermService termService)
        {
            _termService = termService;
        }

        #region "PRIVATE"

        [HttpPost]
        public async Task<IActionResult> CreateTerm([FromBody] NewTermDto request)
        {
            var term = await _termService.CreateTermAsync(request);
            return CreatedAtAction(nameof(GetPrivateTermById), new { id = term.Id }, term);
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> UpdateTerm(int id, [FromBody] UpdateTermDto dto)
        {
            var updated = await _termService.UpdateTermAsync(id, dto);
            return updated is null ? NotFound(new { Message = "Term not found." }) : Ok(updated);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPrivateTerms()
            => Ok(await _termService.GetAllPrivateTermsAsync());

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetPrivateTermById(int id)
        {
            var term = await _termService.GetPrivateTermByIdAsync(id);
            if (term == null)
            {
                return NotFound(new { Message = "Term not found." });
            }
            return Ok(term);
        }

        [HttpPut]
        [Route("toggle-visibility/{id}")]
        public async Task<IActionResult> ToggleVisibility(int id)
        {
            var deactivated = await _termService.ToggleVisibilityAsync(id);
            return deactivated ? Ok() : NotFound(new { Message = "Term not found." });
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> HardDelete(int id)
        {
            var deleted = await _termService.HardDeleteTermAsync(id);
            return deleted ? Ok() : NotFound(new { Message = "Term not found." });
        }
        #endregion

    }
}
