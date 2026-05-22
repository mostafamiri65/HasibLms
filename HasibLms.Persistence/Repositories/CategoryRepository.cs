using HasibLms.Domain.Entities.AcademyEntities;
using HasibLms.Domain.Interfaces;
using HasibLms.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace HasibLms.Persistence.Repositories;

public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
	public CategoryRepository(LmsContext context) : base(context)
	{
	}

	public async Task<IReadOnlyList<Category>> GetAllWithChildrenAsync(CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.Include(c => c.Children)
			.Where(c => !c.IsDeleted)
			.ToListAsync(cancellationToken);
	}

	public async Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.Include(c => c.Children)
			.FirstOrDefaultAsync(c => c.Slug == slug && !c.IsDeleted, cancellationToken);
	}
}