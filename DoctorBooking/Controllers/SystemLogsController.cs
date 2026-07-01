using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace DoctorBooking.Controllers
{
    [ApiController]
    [Route("api/system_logs")]
    [Authorize("Admin")]
    public class SystemLogsController : ControllerBase
    {
        private readonly ISystemLogService service;

        public SystemLogsController(ISystemLogService service)
        {
            this.service = service;
        }

        [HttpGet("get_all_logs")]
        [ProducesResponseType(typeof(GeneicResult<PaginatedResult<SystemLogResponseDTO>>), 200)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllLogsAsync([FromQuery] SystemLogsRequestDTO requestDTO)
        {
            var result = await service.GetAllLogsAsync(requestDTO);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("get_log_by_id/{logId}")]
        [ProducesResponseType(typeof(GeneicResult<SystemLogResponseDTO>), 200)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetLogByIdAsync(long logId)
        {
            var result = await service.GetLogByIdAsync(logId);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("get_logs_by_actor_id/{actorId}")]
        [ProducesResponseType(typeof(GeneicResult<List<SystemLogResponseDTO>>), 200)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetLogsByActorIdAsync(long actorId)
        {
            var result = await service.GetLogsByActorIdAsync(actorId);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("get_logs_for_specific_target")]
        [ProducesResponseType(typeof(GeneicResult<PaginatedResult<SystemLogResponseForSpecificTargetResponseDTO>>),StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetSystemLogsForSpecificTargetAsync([FromQuery] SystemLogsRequestDTO requestDTO)
        {
            var result = await service.GetSystemLogsForSpecificTargetAsync(requestDTO);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
