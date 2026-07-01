using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Models;
using Microsoft.Data.SqlTypes;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repsoitory
{
    public class SystemLogRepository : ISystemLogRepository
    {
        private readonly AppDBContext dbContext;

        public SystemLogRepository(AppDBContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<PaginatedResult<SystemLogResponseDTO>> GetAllLogsAsync(SystemLogsRequestDTO requestDTO )
        {
            var query = dbContext.system_logs.AsQueryable();


            if (!string.IsNullOrEmpty(requestDTO.ActorType))
            {
                query = query.Where(x => x.actor_type == requestDTO.ActorType);
            }

            if (!string.IsNullOrEmpty(requestDTO.Action))
            {
                query = query.Where(x => x.action == requestDTO.Action);
            }


            var result = await query.AsNoTracking()
                                   .Select(x => new SystemLogResponseDTO
                                   {
                                       id = x.id,
                                       actor_type = x.actor_type,
                                       actor_id = x.actor_id,
                                       action = x.action,
                                       target_type = x.target_type,
                                       target_id = x.target_id,
                                       description = x.description,
                                       metadata = x.metadata,
                                       ip_address = x.ip_address,
                                       created_at = x.created_at
                                   })
                                   .Skip((requestDTO.PageNumber - 1) * requestDTO.PageSize)
                                   .Take(requestDTO.PageSize)
                                   .ToListAsync();

           return new PaginatedResult<SystemLogResponseDTO>
           {
               IsSuccess = true,
               message = "Logs retrieved successfully.",
               Data = result,
               Pagination = new PaginationMetadata
               {
                   CurrentPage = requestDTO.PageNumber,
                   PageSize = requestDTO.PageSize,
                   TotalItems = await query.CountAsync()
               }
           };
        }

        public async Task<PaginatedResult<SystemLogResponseForSpecificTargetResponseDTO>>  GetSystemLogsForSpecificTargetAsync(SystemLogsRequestDTO requestDTO)
        {
            var query = dbContext.system_logs.AsQueryable();


            if (!string.IsNullOrEmpty(requestDTO.TargetType))
            {
                query = query.Where(x => x.target_type == requestDTO.TargetType);
            }

            if (requestDTO.TargetId != null)
            {
                query = query.Where(x => x.target_id == requestDTO.TargetId);
            }


            List<SystemLogResponseForSpecificTargetResponseDTO> result = await query.AsNoTracking()
                                   .Skip((requestDTO.PageNumber - 1) * requestDTO.PageSize)
                                   .Take(requestDTO.PageSize)
                                   .Select(x => new SystemLogResponseForSpecificTargetResponseDTO
                                   {
                                       action = x.action,
                                       description = x.description,
                                   })
                                   .ToListAsync();

            return new PaginatedResult<SystemLogResponseForSpecificTargetResponseDTO>
            {
                IsSuccess = true,
                message = "Logs retrieved successfully.",
                Data = result,
                Pagination = new PaginationMetadata
                {
                    CurrentPage = requestDTO.PageNumber,
                    PageSize = requestDTO.PageSize,
                    TotalItems = await query.CountAsync()
                }
            };
        }
    }
}
