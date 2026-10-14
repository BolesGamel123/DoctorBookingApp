using Application.Common.Interfaces;
using Application.DTOs;
using Application.Features;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorBooking.Controllers
{
    [ApiController]
    [Route("api/search_history")]
    [Authorize(Roles = "Patient")]
    public class SearchHistoryController : ControllerBase
    {
        private readonly ISearchHistoryService searchHistoryService;

        public SearchHistoryController(ISearchHistoryService searchHistoryService)
        {
            this.searchHistoryService = searchHistoryService;
        }

        [HttpGet("search_history")]
        [ProducesResponseType(typeof(GeneicResult<PaginatedResult<SearcHistoryDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GeneicResult<PaginatedResult<SearcHistoryDTO>>>> GetSearchHistory([FromQuery] int page, [FromQuery] int pageSize)
        {
            var result = await searchHistoryService.GetSearchHistoryDTO(page, pageSize);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("remove_search_history/{searchHistoryId}")]
        [ProducesResponseType(typeof(GeneicResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveSearchHistory(long searchHistoryId)
        {
            var result = await searchHistoryService.RemoveSearchHistory(searchHistoryId);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("clear_search_history")]
        [ProducesResponseType(typeof(GeneicResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ClearSearchHistory()
        {
            var result = await searchHistoryService.RemoveAllSearchHistory();
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }
    }
}
