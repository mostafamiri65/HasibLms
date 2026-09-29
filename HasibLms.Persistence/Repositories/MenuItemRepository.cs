using HasibLms.Domain.Entities.Settings;
using HasibLms.Domain.Interfaces;
using HasibLms.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace HasibLms.Persistence.Repositories;

public class MenuItemRepository : GenericRepository<MenuItemEntity>, IMenuItemRepository
{
	public MenuItemRepository(LmsContext context) : base(context)
	{
	}

	public async Task<IReadOnlyList<MenuItemEntity>> GetMenuItemsByLocationAsync(string location, CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.Where(m => m.Location == location && !m.IsDeleted && m.ParentId == null)
			.Include(m => m.Children.Where(c => c.IsActive && !c.IsDeleted))
			.OrderBy(m => m.Order)
			.ToListAsync(cancellationToken);
	}

	public async Task<IReadOnlyList<MenuItemEntity>> GetActiveMenuItemsAsync(string location, CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.Where(m => m.Location == location && m.IsActive && !m.IsDeleted && m.ParentId == null)
			.Include(m => m.Children.Where(c => c.IsActive && !c.IsDeleted))
			.OrderBy(m => m.Order)
			.ToListAsync(cancellationToken);
	}

	public async Task<int> GetMaxOrderAsync(string location, CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.Where(m => m.Location == location && !m.IsDeleted)
			.MaxAsync(m => (int?)m.Order) ?? 0;
	}

	public async Task ReorderMenuItemsAsync(List<Guid> ids, CancellationToken cancellationToken = default)
	{
		for (int i = 0; i < ids.Count; i++)
		{
			var item = await _dbSet.FindAsync(new object[] { ids[i] }, cancellationToken);
			if (item != null && !item.IsDeleted)
			{
				item.Order = i + 1;
				item.LastModifiedDate = DateTime.UtcNow;
			}
		}
		await _context.SaveChangesAsync(cancellationToken);
	}
}