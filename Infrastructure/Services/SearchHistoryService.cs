using Application.Common.Interfaces;
using Application.DTOs;
using Application.Features;
using AutoMapper;
using Domain.Entities;
using Domain.Entities;
using Infrastructure.UnitOfWorks;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Services
{
    public class SearchHistoryService : ISearchHistoryService
    {
        private readonly IUnitOfWorks unitOfWork;
        private readonly IMapper mapper;
        private readonly IHttpContextAccessor accessor;

        public SearchHistoryService(IUnitOfWorks unitOfWork,
                                    IMapper mapper,
                                    IHttpContextAccessor accessor)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.accessor = accessor;
        }

        public async Task<GeneicResult<PaginatedResult<SearcHistoryDTO>>> GetSearchHistoryDTO(int page, int pageSize)
        {
            var userId = accessor.HttpContext?.User?.FindFirst("id")?.Value;
            if (userId == null)
                return new GeneicResult<PaginatedResult<SearcHistoryDTO>>
                {
                    IsSuccess = false,
                    Message = "User not authenticated."
                };
            var searchHistoryList = await unitOfWork.search_history.GetPaginationAsync(predicate: sh => sh.user_id == int.Parse(userId),
                                                                                 selector: sh => new SearcHistoryDTO
                                                                                 {
                                                                                     id = sh.id,
                                                                                     SearchText = sh.query,
                                                                                     CreatedAt = sh.created_at
                                                                                 }, page, pageSize);
            return Result.Success<PaginatedResult<SearcHistoryDTO>>(searchHistoryList);
        }

        public async Task<GeneicResult<DoctorSearchResponseDTO>> GetDoctor(DoctorSearchDTO dto)
        {
            if (dto == null)
                return new GeneicResult<DoctorSearchResponseDTO>
                {
                    IsSuccess = false,
                    Message = "Invalid search parameters."
                };

            //. speciality_id is a foreign key to the Specialization table, so we can use it to get the specialization name
            var result = await unitOfWork.doctor_repo.GetByIdAsync(predicate: op => op.full_name == dto.Name &&
                                                                              op.specialty_id == dto.SpecializationId,
                                                                               selector: op => new DoctorSearchResponseDTO
                                                                               {
                                                                                   Id = op.id,
                                                                                   Name = op.full_name,
                                                                                   Rating = op.rating_avg,
                                                                                   Specialization = op.specialty.name
                                                                               });
            var searchHistory = new search_history
            {
                user_id = result.Id,
                created_at = DateTime.UtcNow,
                search_type = "Doctor"
            };

            await unitOfWork.search_history.Create(searchHistory);
            await unitOfWork.SaveAsync();

            return Result.Success<DoctorSearchResponseDTO>(result);
        }

        public async Task<GeneicResult<string>> RemoveSearchHistory(long searchHistoryId)
        {
            var searchHistory = await unitOfWork.search_history.GetByIdAsync(predicate: sh => sh.id == searchHistoryId, selector: sh => sh);
            if (searchHistory == null)
                return Result.Failure<string>("Search history not found.");
            await unitOfWork.search_history.Remove(searchHistory);
            await unitOfWork.SaveAsync();

            return Result.Success<string>("Search history removed successfully.");
        }

        public async Task<GeneicResult<string>> RemoveAllSearchHistory()
        {
            var userId = accessor.HttpContext?.User?.FindFirst("id")?.Value;
            if (userId == null)
                return new GeneicResult<string>
                {
                    IsSuccess = false,
                    Message = "User not authenticated."
                };

            var searchHistories = await unitOfWork.search_history.GetAllUsingPredicateAsync(predicate: sh => sh.user_id == long.Parse(userId));
            if (searchHistories == null || !searchHistories.Any())
                return Result.Failure<string>("No search history found to remove.");

            //. not best practice to remove all search history one by one, but for now we will do it this way (old practice)
            //foreach (var searchHistory in searchHistories)
            //{
            //    await unitOfWork.search_history.Remove(searchHistory);
            //}

            //. this way using the Execute Remove RangeAsync method is more efficient and better practice
            await unitOfWork.search_history.RemoveRangeAsync(predicate: sh => sh.user_id == long.Parse(userId));
            await unitOfWork.SaveAsync();
            return Result.Success<string>("All search history removed successfully.");
        }
    }
}