using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Interfaces
{
    public interface IDoctorFavoriteService
    {
        Task<GeneicResult<FavoriteResponseDTO>> Add_Favorite_Doctor(long doctor_Id);
        Task<GeneicResult<string>> Remove_Favorite_Doctor(long doctor_Id);
        Task<GeneicResult<PaginatedResult<FavoriteDoctorDTO>>> GetFavorites(int page, int pageSize);
        Task<GeneicResult<CheckFavoriteDTO>> check_favorite(long doctor_id);
    }
}
