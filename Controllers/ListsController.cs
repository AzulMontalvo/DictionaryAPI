using DictionaryAPI.Interfaces;
using DictionaryAPI.Models.DTOs.List;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DictionaryAPI.Controllers
{
    [ApiController]
    [Route("api/lists")]
    public class ListsController : ControllerBase
    {
        private readonly IListService _listService;

        public ListsController(IListService listService)
        {
            _listService = listService;
        }
        #region "PUBLIC"
        [HttpGet("featured")]
        public async Task<IActionResult> GetFeaturedLists()
        {
            var results = await _listService.GetFeaturedListsAsync();
            return Ok(results);
        }

        [HttpGet("public")]
        public async Task<IActionResult> GetPublicLists()
        {
            var results = await _listService.GetPublicListsAsync();
            return Ok(results);
        }
        #endregion


        #region "SHARED"
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetListById(int id)
        {
            var list = await _listService.GetListByIdAsync(id);
            return list is null ? NotFound() : Ok(list);
        }
        #endregion

        #region "NEEDS AUTH"
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetUserLists()
            => Ok(await _listService.GetUserListsAsync());

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateList([FromBody] NewListDto dto)
        {
            var list = await _listService.CreateListAsync(dto);
            return CreatedAtAction(nameof(GetListById), new { id = list.Id }, list);
        }

        [Authorize]
        [HttpPatch("{id:int}/rename")]
        public async Task<IActionResult> RenameList(int id, [FromBody] string newName)
        {
            if (string.IsNullOrWhiteSpace(newName)) return BadRequest("Empty name");
            var list = await _listService.RenameListAsync(id, newName);
            return list is null ? NotFound() : Ok(list);
        }

        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteList(int id)
        {
            var deleted = await _listService.DeleteListAsync(id);
            return deleted ? NoContent() : NotFound();
        }

        [Authorize]
        [HttpPost("{id:int}/terms")]
        public async Task<IActionResult> AddTerm(int id, [FromBody] TermListDto dto)
        {
            var result = await _listService.AddTermAsync(id, dto.TermId);
            return result ? Ok() : BadRequest("Term doesn't exist or it's already on the list.");
        }

        [Authorize]
        [HttpDelete("{id:int}/terms/{termId:int}")]
        public async Task<IActionResult> RemoveTerm(int id, int termId)
        {
            var result = await _listService.RemoveTermAsync(id, termId);
            return result ? NoContent() : NotFound();
        }
        #endregion

        #region "ADMIN"
        [Authorize]
        [HttpPost("featured/schedule")]
        public async Task<IActionResult> ScheduleFeatured([FromBody] NewFeaturedListDto dto)
        {
            try
            {
                var success = await _listService.CreateScheduledListAsync(dto);
                if (!success)
                {
                    return NotFound("La lista especificada no existe o no está marcada como visible.");
                }

                return Ok(new { message = "Colección programada con éxito." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("featured/schedule")]
        public async Task<IActionResult> GetFeaturedSchedule()
        {
            var schedule = await _listService.GetFeaturedScheduleAsync();
            return Ok(schedule);
        }
        #endregion
    }
}