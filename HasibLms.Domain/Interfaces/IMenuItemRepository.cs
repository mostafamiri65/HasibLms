using HasibLms.Domain.Entities.Settings;

namespace HasibLms.Domain.Interfaces;

public interface IMenuItemRepository : IGenericRepository<MenuItemEntity>
{
	Task<IReadOnlyList<MenuItemEntity>> GetMenuItemsByLocationAsync(string location, CancellationToken cancellationToken = default);
	Task<IReadOnlyList<MenuItemEntity>> GetActiveMenuItemsAsync(string location, CancellationToken cancellationToken = default);
	Task<int> GetMaxOrderAsync(string location, CancellationToken cancellationToken = default);
	Task ReorderMenuItemsAsync(List<Guid> ids, CancellationToken cancellationToken = default);
}