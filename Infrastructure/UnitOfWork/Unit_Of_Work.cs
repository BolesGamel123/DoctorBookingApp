using Application.Common.Interfaces;
using Application.Features;
using Domain.Entities;
using Domain.Entities;
using Infrastructure.Persistence;
using Infrastructure.Repsoitory;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.UnitOfWorks
{
    public class Unit_Of_Work : IUnitOfWorks
    {
        private readonly AppDbContext dbContext;
        public IGenericRepository<favorite> favorite_repo { get; }
        public IGenericRepository<search_history> search_history { get; }
        public IGenericRepository<doctor> doctor_repo { get; }
        public ISystemLogRepository systemLogrepo { get; }
        public IGenericRepository<system_log> System_log_repo { get; }

        public Unit_Of_Work(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
            var favorites_repo = new GenericRepository<favorite>(dbContext);
            var doctor_repo = new GenericRepository<doctor>(dbContext);
            var search_history = new GenericRepository<search_history>(dbContext);
            var systemlog_repo = new SystemLogRepository(dbContext);
            var logs_repo = new GenericRepository<system_log>(dbContext);
        }

        public void Dispose()
        {
            dbContext.Dispose();
        }

        public async Task<int> SaveAsync()
        {
            return await dbContext.SaveChangesAsync();
        }
    }
}