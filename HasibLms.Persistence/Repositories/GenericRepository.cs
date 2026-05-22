using HasibLms.Domain.Entities;
using HasibLms.Domain.Interfaces;
using HasibLms.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace HasibLms.Persistence.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
	protected readonly LmsContext _context;
	protected readonly DbSet<T> _dbSet;

	public GenericRepository(LmsContext context)
	{
		_context = context;
		_dbSet = context.Set<T>();
	}

	public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
		=> await _dbSet.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, cancellationToken);

	public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
		=> await _dbSet.Where(e => !e.IsDeleted).ToListAsync(cancellationToken);

	public virtual async Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
		=> await _dbSet.Where(e => !e.IsDeleted).Where(predicate).ToListAsync(cancellationToken);

	public virtual async Task<T?> GetSingleAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
		=> await _dbSet.Where(e => !e.IsDeleted).FirstOrDefaultAsync(predicate, cancellationToken);

	public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
	{
		entity.Id = Guid.NewGuid();
		entity.CreatedDate = DateTime.UtcNow;
		entity.LastModifiedDate = DateTime.UtcNow;
		await _dbSet.AddAsync(entity, cancellationToken);
		await _context.SaveChangesAsync(cancellationToken);
		return entity;
	}

	public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
	{
		entity.LastModifiedDate = DateTime.UtcNow;
		_dbSet.Update(entity);
		await _context.SaveChangesAsync(cancellationToken);
	}

	public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
	{
		entity.IsDeleted = true;
		entity.LastModifiedDate = DateTime.UtcNow;
		_dbSet.Update(entity);
		await _context.SaveChangesAsync(cancellationToken);
	}

	public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
		=> await _dbSet.Where(e => !e.IsDeleted).AnyAsync(predicate, cancellationToken);

	public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
	{
		var query = _dbSet.Where(e => !e.IsDeleted);
		if (predicate is not null)
			query = query.Where(predicate);
		return await query.CountAsync(cancellationToken);
	}
	// اضافه کنید به کلاس GenericRepository:

	public virtual async Task<IReadOnlyList<T>> GetPagedAsync(
		int page,
		int pageSize,
		Expression<Func<T, bool>>? predicate = null,
		Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
		CancellationToken cancellationToken = default)
	{
		IQueryable<T> query = _dbSet.Where(e => !e.IsDeleted);

		if (predicate != null)
			query = query.Where(predicate);

		if (orderBy != null)
			query = orderBy(query);
		else
			query = query.OrderByDescending(e => e.CreatedDate);

		return await query
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.ToListAsync(cancellationToken);
	}
}