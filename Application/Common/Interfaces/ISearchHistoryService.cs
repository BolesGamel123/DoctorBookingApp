using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Interfaces
{
    public interface ISearchHistoryService
    {
        Task<GeneicResult<DoctorSearchResponseDTO>> GetDoctor(DoctorSearchDTO dto);
        Task<GeneicResult<PaginatedResult<SearcHistoryDTO>>> GetSearchHistoryDTO(int page, int pageSize);
        Task<GeneicResult<string>> RemoveSearchHistory(long searchHistoryId);
        Task<GeneicResult<string>> RemoveAllSearchHistory();
    }
}
