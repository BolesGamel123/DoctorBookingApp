using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Interfaces
{
    public interface ISystemLogRepository
    {
        Task<PaginatedResult<SystemLogResponseDTO>> GetAllLogsAsync(SystemLogsRequestDTO requestDTO);
        Task<PaginatedResult<SystemLogResponseDTO>> GetLogsByActorIdAsync(long actorId, int pageNumber, int pageSize);
        //. to be able to use the pagination 
        Task<PaginatedResult<SystemLogResponseForSpecificTargetResponseDTO>> GetSystemLogsForSpecificTargetAsync(SystemLogsRequestDTO requestDTO);
    }
}
