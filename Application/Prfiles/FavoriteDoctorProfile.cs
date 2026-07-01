using Application.DTOs;
using AutoMapper;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Runtime;
using System.Text;

namespace Application.Prfiles
{
    public class FavoriteDoctorProfile : Profile
    {
        public FavoriteDoctorProfile()
        {
            CreateMap<favorite, FavoriteResponseDTO>();
            CreateMap<favorite, CreateFavouriteDoctorDTO>();
        }
    }
}
