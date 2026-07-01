using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Models;
using Infrastructure.UnitOfWorks;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Services
{
    public class SystemLogService : ISystemLogService
    {
        private readonly IUnitOfWorks unitOfWork;
        private readonly IMapper mapper;

        public SystemLogService(IUnitOfWorks unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<GeneicResult<PaginatedResult<SystemLogResponseDTO>>> GetAllLogsAsync(SystemLogsRequestDTO requestDTO)
        {
            var logs = await unitOfWork.systemLogrepo.GetAllLogsAsync(requestDTO);

            //. Mapping list of SystemLog to SystemLogResponseDTO
            var mappedLogs = mapper.Map<List<SystemLogResponseDTO>>(logs.Data);
            var paginatedResult = new PaginatedResult<SystemLogResponseDTO>
            {
                IsSuccess = true,
                message = "Logs retrieved successfully",
                Data = mappedLogs,
                Pagination = logs.Pagination,
                TotalCount = logs.TotalCount
            };
            return new GeneicResult<PaginatedResult<SystemLogResponseDTO>>
            {
                IsSuccess = true,
                Message = "Logs retrieved successfully",
                Data = paginatedResult
            };
        }

        public async Task<GeneicResult<SystemLogResponseDTO>> GetLogByIdAsync(long logId)
        {
            var log = await unitOfWork.System_log_repo.Get(logId);
            if (log == null)
            {
                return new GeneicResult<SystemLogResponseDTO>
                {
                    IsSuccess = false,
                    Message = "Log not found",
                    Data = null
                };
            }
            var mappedLog = mapper.Map<SystemLogResponseDTO>(log);
            return new GeneicResult<SystemLogResponseDTO>
            {
                IsSuccess = true,
                Message = "Log retrieved successfully",
                Data = mappedLog
            };
        }

        public async Task<GeneicResult<List<SystemLogResponseDTO>>> GetLogsByActorIdAsync(long actor_id)
        {
            var logs = await unitOfWork.System_log_repo.GetAllUsingPredicateAsync(predicate: op => op.actor_id == actor_id);
            if (logs == null || logs.Count == 0)
            {
                return new GeneicResult<List<SystemLogResponseDTO>>
                {
                    IsSuccess = false,
                    Message = "No logs found for the specified actor ID",
                    Data = null
                };
            }
            var mappedLogs = mapper.Map<List<SystemLogResponseDTO>>(logs);
            return new GeneicResult<List<SystemLogResponseDTO>>
            {
                IsSuccess = true,
                Message = "Logs retrieved successfully",
                Data = mappedLogs // Assuming you want to return the first log for the actor
            };
        }

        public async Task<GeneicResult<PaginatedResult<SystemLogResponseForSpecificTargetResponseDTO>>> GetSystemLogsForSpecificTargetAsync(SystemLogsRequestDTO requestDTO)
        {
            var logs = await unitOfWork.systemLogrepo.GetSystemLogsForSpecificTargetAsync(requestDTO);
            if (logs == null || logs.Data == null || logs.Data.Count == 0)
            {
                return new GeneicResult<PaginatedResult<SystemLogResponseForSpecificTargetResponseDTO>>
                {
                    IsSuccess = false,
                    Message = "No logs found for the specified target",
                    Data = null
                };
            }
            return new GeneicResult<PaginatedResult<SystemLogResponseForSpecificTargetResponseDTO>>
            {
                IsSuccess = true,
                Message = "Logs retrieved successfully",
                Data = logs
            };
        }

        public async Task<GeneicResult<CreateSystemLogResponseDTO>> CreateSystemLogAsync(CreateSystemLogRequestDTO dTO)
        {
            var logEntity = mapper.Map<system_log>(dTO);
            logEntity.created_at = DateTime.UtcNow; // Set the created_at timestamp

            await unitOfWork.System_log_repo.Create(logEntity);
            await unitOfWork.SaveAsync();

            var responseDTO = mapper.Map<CreateSystemLogResponseDTO>(logEntity);
            return new GeneicResult<CreateSystemLogResponseDTO>
            {
                IsSuccess = true,
                Message = "System log created successfully",
                Data = responseDTO
            };
        }
    }
}
