using HasibLms.Shared.DTOs.Auth;
using HasibLms.Shared.DTOs.Settings;

namespace HasibLms.Domain.Interfaces;

public interface IMenuService
{
	Task<MenuSettingsDto> GetMenuSettingsAsync(CancellationToken cancellationToken = default);
	Task<List<MenuItemDto>> GetHeaderMenuAsync(CancellationToken cancellationToken = default);
	Task<List<MenuItemDto>> GetFooterMenuAsync(CancellationToken cancellationToken = default);
	Task<AuthResultDto> CreateMenuItemAsync(CreateMenuItemDto model, CancellationToken cancellationToken = default);
	Task<AuthResultDto> UpdateMenuItemAsync(UpdateMenuItemDto model, CancellationToken cancellationToken = default);
	Task<AuthResultDto> DeleteMenuItemAsync(Guid id, CancellationToken cancellationToken = default);
	Task<AuthResultDto> ReorderMenuItemsAsync(string location, List<Guid> ids, CancellationToken cancellationToken = default);
	Task<MenuItemDto?> GetMenuItemByIdAsync(Guid id, CancellationToken cancellationToken = default);
	Task ClearMenuCacheAsync();
}