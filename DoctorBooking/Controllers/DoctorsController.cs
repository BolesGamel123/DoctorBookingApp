using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorBooking.Controllers
{
    [ApiController]
    [Route("api/doctors")]
    [Authorize]
    public class DoctorsController : ControllerBase
    {
        private readonly ISearchHistoryService service;

        public DoctorsController(ISearchHistoryService service)
        {
            this.service = service;
        }

        [HttpGet("search")]
        [ProducesResponseType(typeof(GeneicResult<DoctorSearchResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GeneicResult<DoctorSearchResponseDTO>>> SearchDoctors([FromQuery] DoctorSearchDto dto)
        {
            var result = await service.GetDoctor(dto);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }
    }
}
