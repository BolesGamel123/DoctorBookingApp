using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces
{
    public interface ISystemLogRepository
    {
        Task<PaginatedResult<SystemLogResponseDTO>> GetAllLogsAsync(SystemLogsRequestDTO requestDTO);
        //. to be able to use the pagination 
        Task<PaginatedResult<SystemLogResponseForSpecificTargetResponseDTO>> GetSystemLogsForSpecificTargetAsync(SystemLogsRequestDTO requestDTO); 
    }
}
