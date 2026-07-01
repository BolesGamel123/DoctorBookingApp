using Application.DTOs;
using AutoMapper;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Application.Prfiles
{
    public class SystemLogsProfile : Profile
    {
        public SystemLogsProfile()
        {
            CreateMap<system_log, SystemLogResponseDTO>();
            CreateMap<CreateSystemLogRequestDTO, system_log>();
            CreateMap<system_log, CreateSystemLogResponseDTO>();

        }
    }
}
