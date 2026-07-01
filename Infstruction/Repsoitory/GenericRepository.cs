using Application.DTOs;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Infrastructure.Repsoitory
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        private readonly AppDBContext dbContext;

        public GenericRepository(AppDBContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task Create(TEntity data)
        {
            await dbContext.Set<TEntity>().AddAsync(data);
        }

        public async Task Update(TEntity data)
        {
            dbContext.Set<TEntity>().Update(data);
        }

        public async Task Remove(TEntity data)
        {
            dbContext.Set<TEntity>().Remove(data);
        }

        public async Task RemoveRangeAsync(Expression<Func<TEntity, bool>> predicate) 
        {
            await dbContext.Set<TEntity>().Where(predicate).ExecuteDeleteAsync();
        }

        public async Task<TEntity> Get(long Id)
        {
            var result = await dbContext.Set<TEntity>().FindAsync(Id);
            return result;
        }

        public async Task<IEnumerable<TEntity>> GetAll()
        {
            var result = await dbContext.Set<TEntity>().ToListAsync();
            return result;
        }

        public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> validate)
        {
            IQueryable<TEntity> query = dbContext.Set<TEntity>();

            return await query.AnyAsync(validate);
        }

        public async Task<TResult> GetByIdAsync<TResult>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TResult>>? selector)
        {
            var result = await dbContext.Set<TEntity>().Where(predicate).Select(selector).FirstOrDefaultAsync();
            return result;
        }

        public async Task<List<TEntity>> GetAllUsingPredicateAsync(Expression<Func<TEntity, bool>> predicate)
        {
            var result = await dbContext.Set<TEntity>().Where(predicate).ToListAsync();
            return result;
        }

        public async Task<TResult> GetById<TResult>(Expression<Func<TEntity, bool>> predicate = null,
                                                    Expression<Func<TEntity, TResult>> selector = null,
                                                    params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = dbContext.Set<TEntity>().AsQueryable();

            //. Generic Eager loading
            if (includes != null && includes.Length > 0)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);//. updating value of the query of type IQuerable 
                }
            }

            //.filter first
            query = query.Where(predicate);

            //. selector and returning generic type of entities 
            if (selector != null)
            {
                return await query.AsNoTracking().Select(selector).FirstOrDefaultAsync();
            }

            //. if the type of the Result value is the same type of the Entity return
            //. the first value with the filtering with id and includes (you don't need to the selector here)

            if (typeof(TResult) == typeof(TEntity))
            {
                var entity = await query.AsNoTracking().FirstOrDefaultAsync();

                return (TResult)(object)entity;
            }

            throw new InvalidOperationException(
                            "Selector is required when TResult is not TEntity"
                        );
        }

        public async Task<PaginatedResult<TResult>> GetPaginationAsync<TResult>(
                                                    Expression<Func<TEntity, bool>> predicate,
                                                    Expression<Func<TEntity, TResult>> selector,
                                                    int page,
                                                    int pageSize,
                                                    string message = null,
                                                    params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = dbContext.Set<TEntity>();

            if (includes != null && includes.Length > 1)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            if (predicate != null)
                query = query.Where(predicate);

            var totalItems = await query.CountAsync();

            //.meaning that if the current page is 2 and page size is 5 that's meaning that id will skip the first 5 items and after that
            // . taking the second 5 items and like that 
            var items = await query.AsNoTracking().
                                    Select(selector)
                                   .Skip((page - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync();

            return new PaginatedResult<TResult>
            {
                IsSuccess = true,
                message = message,
                Data = items,
                Pagination = new PaginationMetadata
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
                },
                TotalCount = items.Count
            };
        }
    }
}
