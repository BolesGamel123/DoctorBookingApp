using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces
{
    public interface ISystemLogService
    {
        Task<GeneicResult<PaginatedResult<SystemLogResponseDTO>>> GetAllLogsAsync(SystemLogsRequestDTO requestDTO);
        Task<GeneicResult<SystemLogResponseDTO>> GetLogByIdAsync(long logId);
        Task<GeneicResult<List<SystemLogResponseDTO>>> GetLogsByActorIdAsync(long actor_id);
        Task<GeneicResult<PaginatedResult<SystemLogResponseForSpecificTargetResponseDTO>>> GetSystemLogsForSpecificTargetAsync(SystemLogsRequestDTO requestDTO);
        Task<GeneicResult<CreateSystemLogResponseDTO>> CreateSystemLogAsync(CreateSystemLogRequestDTO dTO);
    }
}
