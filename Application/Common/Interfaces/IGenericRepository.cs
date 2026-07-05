using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Application.Common.Interfaces
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        Task Create(TEntity data);

        Task Update(TEntity data);
        Task Remove(TEntity data);
        Task RemoveRangeAsync(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity> Get(long Id);
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> validate);
        Task<List<TEntity>> GetAllUsingPredicateAsync(Expression<Func<TEntity, bool>> predicate);
        Task<IEnumerable<TEntity>> GetAll();
        Task<TResult> GetByIdAsync<TResult>(
                                             Expression<Func<TEntity, bool>> predicate,
                                             Expression<Func<TEntity, TResult>>? selector);
        Task<TResult> GetById<TResult>(Expression<Func<TEntity, bool>> predicate = null,
                                                    Expression<Func<TEntity, TResult>> selector = null,
                                                    params Expression<Func<TEntity, object>>[] includes);
        Task<PaginatedResult<TResult>> GetPaginationAsync<TResult>(
                                                    Expression<Func<TEntity, bool>> predicate,
                                                    Expression<Func<TEntity, TResult>> selector,
                                                    int page,
                                                    int pageSize,
                                                    string message = null,
                                                    params Expression<Func<TEntity, object>>[] includes);
    }
}
