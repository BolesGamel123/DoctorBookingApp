using Application.Interfaces;
using Domain.Models;
using Infrastructure.Repsoitory;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.UnitOfWorks
{
    public interface IUnitOfWorks : IDisposable
    {
        IGenericRepository<favorite> favorite_repo { get; }
        IGenericRepository<search_history> search_history { get; }
        IGenericRepository<doctor> doctor_repo { get; }
        ISystemLogRepository systemLogrepo { get; }
        IGenericRepository<system_log> System_log_repo { get; }
        Task<int> SaveAsync();
    }
}
