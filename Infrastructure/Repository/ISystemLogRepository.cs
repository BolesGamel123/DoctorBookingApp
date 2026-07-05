using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repsoitory
{
    public class SystemLogRepository : ISystemLogRepository
    {
        private readonly AppDbContext _dbContext;

        public SystemLogRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // ── Get All Logs (filtered + paginated) ───────────────────────────────
        public async Task<PaginatedResult<SystemLogResponseDTO>> GetAllLogsAsync(SystemLogsRequestDTO requestDTO)
        {
            var query = _dbContext.system_logs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(requestDTO.ActorType))
                query = query.Where(x => x.actor_type == requestDTO.ActorType);

            if (!string.IsNullOrEmpty(requestDTO.Action))
                query = query.Where(x => x.action == requestDTO.Action);

            // Run COUNT and page-data queries concurrently — one DB round-trip each
            var countTask = query.CountAsync();
            var itemsTask = query
                .Select(x => new SystemLogResponseDTO
                {
                    id          = x.id,
                    actor_type  = x.actor_type,
                    actor_id    = x.actor_id,
                    action      = x.action,
                    target_type = x.target_type,
                    target_id   = x.target_id,
                    description = x.description,
                    metadata    = x.metadata,
                    ip_address  = x.ip_address,
                    created_at  = x.created_at
                })
                .Skip((requestDTO.PageNumber - 1) * requestDTO.PageSize)
                .Take(requestDTO.PageSize)
                .ToListAsync();

            await Task.WhenAll(countTask, itemsTask);

            return new PaginatedResult<SystemLogResponseDTO>
            {
                IsSuccess = true,
                message   = "Logs retrieved successfully.",
                Data      = itemsTask.Result,
                pagination = new PaginationMetadata
                {
                    CurrentPage = requestDTO.PageNumber,
                    PageSize    = requestDTO.PageSize,
                    TotalItems  = countTask.Result,
                    TotalPages  = (int)Math.Ceiling(countTask.Result / (double)requestDTO.PageSize)
                }
            };
        }

        // ── Get Logs by Actor ID (paginated — was unbounded before) ──────────
        public async Task<PaginatedResult<SystemLogResponseDTO>> GetLogsByActorIdAsync(
            long actorId, int pageNumber, int pageSize)
        {
            var query = _dbContext.system_logs
                .AsNoTracking()
                .Where(x => x.actor_id == actorId);

            // Run COUNT and page-data queries concurrently
            var countTask = query.CountAsync();
            var itemsTask = query
                .OrderByDescending(x => x.created_at)
                .Select(x => new SystemLogResponseDTO
                {
                    id          = x.id,
                    actor_type  = x.actor_type,
                    actor_id    = x.actor_id,
                    action      = x.action,
                    target_type = x.target_type,
                    target_id   = x.target_id,
                    description = x.description,
                    metadata    = x.metadata,
                    ip_address  = x.ip_address,
                    created_at  = x.created_at
                })
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            await Task.WhenAll(countTask, itemsTask);

            return new PaginatedResult<SystemLogResponseDTO>
            {
                IsSuccess = true,
                message   = "Logs retrieved successfully.",
                Data      = itemsTask.Result,
                pagination = new PaginationMetadata
                {
                    CurrentPage = pageNumber,
                    PageSize    = pageSize,
                    TotalItems  = countTask.Result,
                    TotalPages  = (int)Math.Ceiling(countTask.Result / (double)pageSize)
                }
            };
        }

        // ── Get Logs for a Specific Target (paginated) ───────────────────────
        public async Task<PaginatedResult<SystemLogResponseForSpecificTargetResponseDTO>> GetSystemLogsForSpecificTargetAsync(
            SystemLogsRequestDTO requestDTO)
        {
            var query = _dbContext.system_logs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(requestDTO.TargetType))
                query = query.Where(x => x.target_type == requestDTO.TargetType);

            if (requestDTO.TargetId != null)
                query = query.Where(x => x.target_id == requestDTO.TargetId);

            // Run COUNT and page-data queries concurrently
            var countTask = query.CountAsync();
            var itemsTask = query
                .OrderByDescending(x => x.created_at)
                .Skip((requestDTO.PageNumber - 1) * requestDTO.PageSize)
                .Take(requestDTO.PageSize)
                .Select(x => new SystemLogResponseForSpecificTargetResponseDTO
                {
                    action      = x.action,
                    description = x.description,
                })
                .ToListAsync();

            await Task.WhenAll(countTask, itemsTask);

            return new PaginatedResult<SystemLogResponseForSpecificTargetResponseDTO>
            {
                IsSuccess = true,
                message   = "Logs retrieved successfully.",
                Data      = itemsTask.Result,
                pagination = new PaginationMetadata
                {
                    CurrentPage = requestDTO.PageNumber,
                    PageSize    = requestDTO.PageSize,
                    TotalItems  = countTask.Result,
                    TotalPages  = (int)Math.Ceiling(countTask.Result / (double)requestDTO.PageSize)
                }
            };
        }
    }
}
