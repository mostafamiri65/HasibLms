using HasibLms.Domain.Entities;
using System.Linq.Expressions;

namespace HasibLms.Domain.Interfaces;

public interface IGenericRepository<T> where T : BaseEntity
{
	Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
	Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
	Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
	Task<T?> GetSingleAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
	Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
	Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
	Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
	Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
	Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
	Task<IReadOnlyList<T>> GetPagedAsync(
		int page,
		int pageSize,
		Expression<Func<T, bool>>? predicate = null,
		Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
		CancellationToken cancellationToken = default);
}