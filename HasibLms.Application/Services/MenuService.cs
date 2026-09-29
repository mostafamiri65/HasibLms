using HasibLms.Domain.Entities.Settings;
using HasibLms.Domain.Interfaces;
using HasibLms.Shared.DTOs.Auth;
using HasibLms.Shared.DTOs.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace HasibLms.Application.Services;

public class MenuService : IMenuService
{
	private readonly IMenuItemRepository _menuItemRepository;
	private readonly IMemoryCache _cache;
	private readonly ILogger<MenuService> _logger;
	private const string HeaderMenuCacheKey = "header_menu";
	private const string FooterMenuCacheKey = "footer_menu";

	public MenuService(
		IMenuItemRepository menuItemRepository,
		IMemoryCache cache,
		ILogger<MenuService> logger)
	{
		_menuItemRepository = menuItemRepository;
		_cache = cache;
		_logger = logger;
	}

	public async Task<MenuSettingsDto> GetMenuSettingsAsync(CancellationToken cancellationToken = default)
	{
		return new MenuSettingsDto
		{
			HeaderMenuItems = await GetHeaderMenuAsync(cancellationToken),
			FooterMenuItems = await GetFooterMenuAsync(cancellationToken)
		};
	}

	public async Task<List<MenuItemDto>> GetHeaderMenuAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			if (_cache.TryGetValue(HeaderMenuCacheKey, out List<MenuItemDto>? cached) && cached != null)
				return cached;

			var items = await _menuItemRepository.GetActiveMenuItemsAsync("header", cancellationToken);
			var result = items.Select(MapToDto).ToList();

			_cache.Set(HeaderMenuCacheKey, result, new MemoryCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
				SlidingExpiration = TimeSpan.FromMinutes(10)
			});

			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting header menu");
			return new List<MenuItemDto>();
		}
	}

	public async Task<List<MenuItemDto>> GetFooterMenuAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			if (_cache.TryGetValue(FooterMenuCacheKey, out List<MenuItemDto>? cached) && cached != null)
				return cached;

			var items = await _menuItemRepository.GetActiveMenuItemsAsync("footer", cancellationToken);
			var result = items.Select(MapToDto).ToList();

			_cache.Set(FooterMenuCacheKey, result, new MemoryCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
				SlidingExpiration = TimeSpan.FromMinutes(10)
			});

			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting footer menu");
			return new List<MenuItemDto>();
		}
	}

	public async Task<AuthResultDto> CreateMenuItemAsync(CreateMenuItemDto model, CancellationToken cancellationToken = default)
	{
		try
		{
			var maxOrder = await _menuItemRepository.GetMaxOrderAsync(model.Location, cancellationToken);

			var item = new MenuItemEntity
			{
				Title = model.Title,
				Url = model.Url,
				Icon = model.Icon,
				Location = model.Location,
				Target = model.Target ?? "_self",
				CssClass = model.CssClass,
				ParentId = model.ParentId,
				Order = maxOrder + 1,
				IsActive = true,
				CreatedDate = DateTime.UtcNow,
				LastModifiedDate = DateTime.UtcNow
			};

			await _menuItemRepository.AddAsync(item, cancellationToken);
			await ClearMenuCacheAsync();

			_logger.LogInformation("Menu item created: {Title}", model.Title);

			return new AuthResultDto
			{
				Succeeded = true,
				Message = "منو با موفقیت اضافه شد",
				Data = item.Id
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error creating menu item");
			return new AuthResultDto
			{
				Succeeded = false,
				Message = "خطا در ایجاد منو"
			};
		}
	}

	public async Task<AuthResultDto> UpdateMenuItemAsync(UpdateMenuItemDto model, CancellationToken cancellationToken = default)
	{
		try
		{
			var item = await _menuItemRepository.GetByIdAsync(model.Id, cancellationToken);
			if (item == null)
			{
				return new AuthResultDto
				{
					Succeeded = false,
					Message = "منو یافت نشد"
				};
			}

			item.Title = model.Title;
			item.Url = model.Url;
			item.Icon = model.Icon;
			item.Target = model.Target ?? "_self";
			item.CssClass = model.CssClass;
			item.ParentId = model.ParentId;
			item.IsActive = model.IsActive;
			item.LastModifiedDate = DateTime.UtcNow;

			await _menuItemRepository.UpdateAsync(item, cancellationToken);
			await ClearMenuCacheAsync();

			return new AuthResultDto
			{
				Succeeded = true,
				Message = "منو با موفقیت بروزرسانی شد"
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating menu item {Id}", model.Id);
			return new AuthResultDto
			{
				Succeeded = false,
				Message = "خطا در بروزرسانی منو"
			};
		}
	}

	public async Task<AuthResultDto> DeleteMenuItemAsync(Guid id, CancellationToken cancellationToken = default)
	{
		try
		{
			var item = await _menuItemRepository.GetByIdAsync(id, cancellationToken);
			if (item == null)
			{
				return new AuthResultDto
				{
					Succeeded = false,
					Message = "منو یافت نشد"
				};
			}

			await _menuItemRepository.DeleteAsync(item, cancellationToken);
			await ClearMenuCacheAsync();

			return new AuthResultDto
			{
				Succeeded = true,
				Message = "منو با موفقیت حذف شد"
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deleting menu item {Id}", id);
			return new AuthResultDto
			{
				Succeeded = false,
				Message = "خطا در حذف منو"
			};
		}
	}

	public async Task<AuthResultDto> ReorderMenuItemsAsync(string location, List<Guid> ids, CancellationToken cancellationToken = default)
	{
		try
		{
			await _menuItemRepository.ReorderMenuItemsAsync(ids, cancellationToken);
			await ClearMenuCacheAsync();

			return new AuthResultDto
			{
				Succeeded = true,
				Message = "ترتیب منوها با موفقیت ذخیره شد"
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error reordering menu items");
			return new AuthResultDto
			{
				Succeeded = false,
				Message = "خطا در تغییر ترتیب منوها"
			};
		}
	}

	public async Task<MenuItemDto?> GetMenuItemByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var item = await _menuItemRepository.GetByIdAsync(id, cancellationToken);
		return item != null ? MapToDto(item) : null;
	}

	public async Task ClearMenuCacheAsync()
	{
		_cache.Remove(HeaderMenuCacheKey);
		_cache.Remove(FooterMenuCacheKey);
		_logger.LogInformation("Menu cache cleared");
		await Task.CompletedTask;
	}

	private MenuItemDto MapToDto(MenuItemEntity item)
	{
		return new MenuItemDto
		{
			Id = item.Id,
			Title = item.Title,
			Url = item.Url,
			Icon = item.Icon,
			Order = item.Order,
			IsActive = item.IsActive,
			Target = item.Target,
			CssClass = item.CssClass,
			ParentId = item.ParentId,
			Location = item.Location,
			Children = item.Children?.Select(MapToDto).ToList() ?? new()
		};
	}
}