using HasibLms.Domain.Entities.Settings;
using HasibLms.Domain.Interfaces;
using HasibLms.Shared.DTOs.Auth;
using HasibLms.Shared.DTOs.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace HasibLms.Application.Services;

public class WhyChooseUsService : IWhyChooseUsService
{
	private readonly IWhyChooseUsRepository _repository;
	private readonly ISiteSettingsRepository _siteSettingsRepository;
	private readonly IMemoryCache _cache;
	private readonly ILogger<WhyChooseUsService> _logger;
	private const string CacheKey = "why_choose_us_items";
	private const string SettingsCacheKey = "why_choose_us_settings";

	public WhyChooseUsService(
		IWhyChooseUsRepository repository,
		ISiteSettingsRepository siteSettingsRepository,
		IMemoryCache cache,
		ILogger<WhyChooseUsService> logger)
	{
		_repository = repository;
		_siteSettingsRepository = siteSettingsRepository;
		_cache = cache;
		_logger = logger;
	}

	public async Task<WhyChooseUsSettingsDto> GetSettingsAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			if (_cache.TryGetValue(SettingsCacheKey, out WhyChooseUsSettingsDto? cached) && cached != null)
				return cached;

			var settings = await _siteSettingsRepository.GetSettingsAsync(cancellationToken);
			var items = await GetActiveItemsForHomePageAsync(cancellationToken);

			var result = new WhyChooseUsSettingsDto
			{
				ShowSection = settings?.IsSiteActive ?? true,
				SectionTitle = settings?.WhyChooseUsTitle ?? "چرا باید ما را انتخاب کنید؟",
				SectionSubtitle = settings?.WhyChooseUsSubtitle,
				Items = items.ToList()
			};

			_cache.Set(SettingsCacheKey, result, new MemoryCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
			});

			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting why choose us settings");
			return new WhyChooseUsSettingsDto();
		}
	}

	public async Task<List<WhyChooseUsItemDto>> GetActiveItemsForHomePageAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			if (_cache.TryGetValue(CacheKey, out List<WhyChooseUsItemDto>? cached) && cached != null)
				return cached;

			var items = await _repository.GetActiveItemsForHomePageAsync(cancellationToken);
			var result = items.Select(MapToDto).ToList();

			_cache.Set(CacheKey, result, new MemoryCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
				SlidingExpiration = TimeSpan.FromMinutes(10)
			});

			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting active why choose us items");
			return new List<WhyChooseUsItemDto>();
		}
	}

	public async Task<AuthResultDto> CreateItemAsync(CreateWhyChooseUsItemDto model, CancellationToken cancellationToken = default)
	{
		try
		{
			var items = await _repository.GetAllAsync(cancellationToken);
			var maxOrder = items.Any() ? items.Max(i => i.Order) : 0;

			var item = new WhyChooseUsItem
			{
				Title = model.Title,
				Description = model.Description,
				Icon = model.Icon,
				Order = maxOrder + 1,
				IsActive = model.IsActive,
				ShowOnHomePage = model.ShowOnHomePage,
				BackgroundColor = model.BackgroundColor,
				IconColor = model.IconColor,
				CreatedDate = DateTime.UtcNow,
				LastModifiedDate = DateTime.UtcNow
			};

			await _repository.AddAsync(item, cancellationToken);
			await ClearCacheAsync();

			return new AuthResultDto
			{
				Succeeded = true,
				Message = "آیتم با موفقیت اضافه شد",
				Data = item.Id
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error creating why choose us item");
			return new AuthResultDto
			{
				Succeeded = false,
				Message = "خطا در ایجاد آیتم"
			};
		}
	}

	public async Task<AuthResultDto> UpdateItemAsync(UpdateWhyChooseUsItemDto model, CancellationToken cancellationToken = default)
	{
		try
		{
			var item = await _repository.GetByIdAsync(model.Id, cancellationToken);
			if (item == null)
			{
				return new AuthResultDto
				{
					Succeeded = false,
					Message = "آیتم یافت نشد"
				};
			}

			item.Title = model.Title;
			item.Description = model.Description;
			item.Icon = model.Icon;
			item.IsActive = model.IsActive;
			item.ShowOnHomePage = model.ShowOnHomePage;
			item.BackgroundColor = model.BackgroundColor;
			item.IconColor = model.IconColor;
			item.LastModifiedDate = DateTime.UtcNow;

			await _repository.UpdateAsync(item, cancellationToken);
			await ClearCacheAsync();

			return new AuthResultDto
			{
				Succeeded = true,
				Message = "آیتم با موفقیت بروزرسانی شد"
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating why choose us item {Id}", model.Id);
			return new AuthResultDto
			{
				Succeeded = false,
				Message = "خطا در بروزرسانی آیتم"
			};
		}
	}

	public async Task<AuthResultDto> DeleteItemAsync(Guid id, CancellationToken cancellationToken = default)
	{
		try
		{
			var item = await _repository.GetByIdAsync(id, cancellationToken);
			if (item == null)
			{
				return new AuthResultDto
				{
					Succeeded = false,
					Message = "آیتم یافت نشد"
				};
			}

			await _repository.DeleteAsync(item, cancellationToken);
			await ClearCacheAsync();

			return new AuthResultDto
			{
				Succeeded = true,
				Message = "آیتم با موفقیت حذف شد"
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deleting why choose us item {Id}", id);
			return new AuthResultDto
			{
				Succeeded = false,
				Message = "خطا در حذف آیتم"
			};
		}
	}

	public async Task<AuthResultDto> ReorderItemsAsync(List<Guid> ids, CancellationToken cancellationToken = default)
	{
		try
		{
			for (int i = 0; i < ids.Count; i++)
			{
				var item = await _repository.GetByIdAsync(ids[i], cancellationToken);
				if (item != null && !item.IsDeleted)
				{
					item.Order = i + 1;
					item.LastModifiedDate = DateTime.UtcNow;
					await _repository.UpdateAsync(item, cancellationToken);
				}
			}

			await ClearCacheAsync();

			return new AuthResultDto
			{
				Succeeded = true,
				Message = "ترتیب آیتم‌ها با موفقیت ذخیره شد"
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error reordering why choose us items");
			return new AuthResultDto
			{
				Succeeded = false,
				Message = "خطا در تغییر ترتیب آیتم‌ها"
			};
		}
	}

	public async Task<AuthResultDto> ToggleShowSectionAsync(bool show, CancellationToken cancellationToken = default)
	{
		try
		{
			var settings = await _siteSettingsRepository.GetSettingsAsync(cancellationToken);
			if (settings != null)
			{
				settings.IsSiteActive = show;
				settings.LastModifiedDate = DateTime.UtcNow;
				await _siteSettingsRepository.UpdateAsync(settings, cancellationToken);

				_cache.Remove(SettingsCacheKey);

				return new AuthResultDto
				{
					Succeeded = true,
					Message = show ? "بخش نمایش داده می‌شود" : "بخش مخفی شد"
				};
			}

			return new AuthResultDto
			{
				Succeeded = false,
				Message = "تنظیمات یافت نشد"
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error toggling why choose us section");
			return new AuthResultDto
			{
				Succeeded = false,
				Message = "خطا در تغییر وضعیت بخش"
			};
		}
	}

	public async Task<WhyChooseUsItemDto?> GetItemByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var item = await _repository.GetByIdAsync(id, cancellationToken);
		return item != null ? MapToDto(item) : null;
	}

	private async Task ClearCacheAsync()
	{
		_cache.Remove(CacheKey);
		_cache.Remove(SettingsCacheKey);
		_logger.LogInformation("Why choose us cache cleared");
		await Task.CompletedTask;
	}

	private WhyChooseUsItemDto MapToDto(WhyChooseUsItem item)
	{
		return new WhyChooseUsItemDto
		{
			Id = item.Id,
			Title = item.Title,
			Description = item.Description,
			Icon = item.Icon,
			Order = item.Order,
			IsActive = item.IsActive,
			ShowOnHomePage = item.ShowOnHomePage,
			BackgroundColor = item.BackgroundColor,
			IconColor = item.IconColor
		};
	}
}