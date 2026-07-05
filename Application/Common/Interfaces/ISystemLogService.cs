using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Interfaces
{
    public interface ISystemLogService
    {
        Task<GeneicResult<PaginatedResult<SystemLogResponseDTO>>> GetAllLogsAsync(SystemLogsRequestDTO requestDTO);
        Task<GeneicResult<SystemLogResponseDTO>> GetLogByIdAsync(long logId);
        Task<GeneicResult<PaginatedResult<SystemLogResponseDTO>>> GetLogsByActorIdAsync(long actor_id, int pageNumber, int pageSize);
        Task<GeneicResult<PaginatedResult<SystemLogResponseForSpecificTargetResponseDTO>>> GetSystemLogsForSpecificTargetAsync(SystemLogsRequestDTO requestDTO);
        Task<GeneicResult<CreateSystemLogResponseDTO>> CreateSystemLogAsync(CreateSystemLogRequestDTO dTO);
    }
}
