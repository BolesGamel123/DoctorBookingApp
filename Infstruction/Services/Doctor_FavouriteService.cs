using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Models;
using Infrastructure.UnitOfWorks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Services
{
    public class Doctor_FavouriteService :IDoctorFavoriteService
    {
        private readonly IHttpContextAccessor accessor;
        private readonly IMapper mapper;
        private readonly IUnitOfWorks unitOfWork;

        public Doctor_FavouriteService(
             IHttpContextAccessor accessor
            ,IMapper mapper,
             IUnitOfWorks unitOfWork)
        {
            this.accessor = accessor;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
        }

        public async Task<GeneicResult<FavoriteResponseDTO>> Add_Favorite_Doctor(long doctor_Id)
        {
            var doctor = await unitOfWork.doctor_repo.Get(doctor_Id);
            if (doctor == null) { Result.Failure<FavoriteResponseDTO>("Doctor Not Found"); } //. Validating if the doctor is existing in list of user's favorites
            try
            {
                //. bringing the doctor_Id for the user (Patient) from the claims that existing in JWT or Token
                if (long.TryParse(accessor.HttpContext.User.FindFirst("sub)")?.Value, out long result))
                {
                    var favoriteDoctor = new favorite
                    {
                        doctor_id = doctor_Id,
                        user_id = result,
                        created_at = DateTime.UtcNow
                    };

                    //. we must use in this case the repositories and UnitOfWork
                    await unitOfWork.favorite_repo.Create(favoriteDoctor);

                    await unitOfWork.SaveAsync(); //. saving the changes to the database usnig unitOfWork

                    var dto = mapper.Map<FavoriteResponseDTO>(favoriteDoctor);
                    dto.DoctorName = doctor.full_name;

                    return Result.Success<FavoriteResponseDTO>(dto);
                }
                else
                    throw new Exception("SomeThing Invalid occurred!! ");
            }catch(ArgumentException ex)
            {
                throw new ArgumentException(ex.Message);
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task<GeneicResult<string>> Remove_Favorite_Doctor(long doctor_Id)
        {
            var doctor = await unitOfWork.doctor_repo.Get(doctor_Id);
            if (doctor == null) { Result.Failure<string>("Doctor Not Found"); } //. Validating if the doctor is existing in list of user's favorites

            var favorite = await unitOfWork.favorite_repo.Get(doctor_Id);
            if (favorite == null)
                return Result.Failure<string>("Doctor already is not exist");

            await unitOfWork.favorite_repo.Remove(favorite);
            await unitOfWork.SaveAsync();

            return Result.Success<string>("doctor removed from favorites");
        }


        public async Task<GeneicResult<PaginatedResult<FavoriteDoctorDTO>>> GetFavorites(int page, int pageSize)
        {
            if (long.TryParse(accessor.HttpContext.User.FindFirst("sub)")?.Value, out long result))
            {
                var favoriteDoctors = await unitOfWork.favorite_repo.GetPaginationAsync(predicate: op => op.user_id == result,
                    selector: op => new FavoriteDoctorDTO
                    {
                        DoctorId = op.doctor_id,
                        DoctorName = op.doctor.full_name,
                        Specialization = op.doctor.specialty.name,
                        Rating = op.doctor.rating_count,
                        Rating_AVG = op.doctor.rating_avg,
                        ImageUrl = op.doctor.profile_photo_url
                    },page: page, pageSize: pageSize);

                return Result.Success<PaginatedResult<FavoriteDoctorDTO>>(favoriteDoctors); //.returning list of favorite Doctor DTO
            }
            return Result.Failure<PaginatedResult<FavoriteDoctorDTO>>("User not found");
        }

        public async Task<GeneicResult<CheckFavoriteDTO>> check_favorite(long doctor_id)
        {
            var doctor = await unitOfWork.doctor_repo.Get(doctor_id);
            if (doctor == null)
                return Result.Failure<CheckFavoriteDTO>("This Doctor is not found");

            bool doctor_exist = await unitOfWork.favorite_repo
                                      .AnyAsync(op => op.doctor_id == doctor_id);
            if(doctor_exist == false)
                return Result.Failure<CheckFavoriteDTO>("This Doctor is not exist in your favorites");

            return Result.Success<CheckFavoriteDTO>(new CheckFavoriteDTO { IsFavorite = doctor_exist });
        }
    }
}
