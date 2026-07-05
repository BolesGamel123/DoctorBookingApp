using Application.Common.Interfaces;
using Application.DTOs;
using Application.Features;
using Domain.Entities;
using Infrastructure.Persistence;
using Infrastructure.Repsoitory;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    /// <summary>
    /// Concrete implementation of ISystemLogService.
    /// Delegates all data access to ISystemLogRepository (which handles pagination
    /// and runs count/data queries concurrently).
    /// </summary>
    public class SystemLogService : ISystemLogService
    {
        private readonly ISystemLogRepository _repo;
        private readonly AppDbContext         _dbContext;
        private readonly IHttpContextAccessor _accessor;

        public SystemLogService(
            ISystemLogRepository repo,
            AppDbContext         dbContext,
            IHttpContextAccessor accessor)
        {
            _repo      = repo;
            _dbContext = dbContext;
            _accessor  = accessor;
        }

        // ── Get all logs (filtered + paginated) ───────────────────────────
        public async Task<GeneicResult<PaginatedResult<SystemLogResponseDTO>>> GetAllLogsAsync(
            SystemLogsRequestDTO requestDTO)
        {
            var data = await _repo.GetAllLogsAsync(requestDTO);
            return Result.Success<PaginatedResult<SystemLogResponseDTO>>(data);
        }

        // ── Get a single log by primary key ───────────────────────────────
        public async Task<GeneicResult<SystemLogResponseDTO>> GetLogByIdAsync(long logId)
        {
            var log = await _dbContext.system_logs
                .AsNoTracking()
                .Where(x => x.id == logId)
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
                .FirstOrDefaultAsync();

            if (log is null)
                return Result.Failure<SystemLogResponseDTO>("Log not found.");

            return Result.Success<SystemLogResponseDTO>(log);
        }

        // ── Get logs by actor ID — now paginated (was unbounded before) ───
        public async Task<GeneicResult<PaginatedResult<SystemLogResponseDTO>>> GetLogsByActorIdAsync(
            long actorId, int pageNumber, int pageSize)
        {
            var data = await _repo.GetLogsByActorIdAsync(actorId, pageNumber, pageSize);
            return Result.Success<PaginatedResult<SystemLogResponseDTO>>(data);
        }

        // ── Get logs for a specific target (paginated) ────────────────────
        public async Task<GeneicResult<PaginatedResult<SystemLogResponseForSpecificTargetResponseDTO>>> GetSystemLogsForSpecificTargetAsync(
            SystemLogsRequestDTO requestDTO)
        {
            var data = await _repo.GetSystemLogsForSpecificTargetAsync(requestDTO);
            return Result.Success<PaginatedResult<SystemLogResponseForSpecificTargetResponseDTO>>(data);
        }

        // ── Create a new system log entry ─────────────────────────────────
        public async Task<GeneicResult<CreateSystemLogResponseDTO>> CreateSystemLogAsync(
            CreateSystemLogRequestDTO dto)
        {
            var ipAddress = _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";

            var entity = new system_log
            {
                actor_type  = dto.actor_type,
                actor_id    = dto.actor_id,
                action      = dto.action,
                target_type = dto.target_type,
                target_id   = dto.target_id,
                description = dto.description,
                metadata    = dto.metadata,
                ip_address  = ipAddress,
                created_at  = DateTime.UtcNow
            };

            _dbContext.system_logs.Add(entity);
            await _dbContext.SaveChangesAsync();

            return Result.Success<CreateSystemLogResponseDTO>(new CreateSystemLogResponseDTO
            {
                id          = entity.id,
                actor_type  = entity.actor_type,
                actor_id    = entity.actor_id,
                action      = entity.action,
                target_type = entity.target_type,
                target_id   = entity.target_id,
                description = entity.description,
                metadata    = entity.metadata,
                ip_address  = entity.ip_address,
                created_at  = entity.created_at
            });
        }
    }
}
