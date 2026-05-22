using HasibLms.Domain.Entities.AcademyEntities;
using HasibLms.Domain.Interfaces;
using HasibLms.Shared.DTOs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace HasibLms.Application.Services;

public class CategoryService : ICategoryService
{
	private readonly ICategoryRepository _categoryRepository;
	private readonly IMemoryCache _cache;
	private readonly ILogger<CategoryService> _logger;
	private const string CacheKey = "all_categories";

	public CategoryService(
		ICategoryRepository categoryRepository,
		IMemoryCache cache,
		ILogger<CategoryService> logger)
	{
		_categoryRepository = categoryRepository;
		_cache = cache;
		_logger = logger;
	}

	public async Task<List<CategoryDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			var cacheKey = $"{CacheKey}_list";

			if (_cache.TryGetValue(cacheKey, out List<CategoryDto>? cached) && cached != null)
				return cached;

			var categories = await _categoryRepository.GetAllAsync(cancellationToken);
			var result = categories.Select(MapToDto).ToList();

			_cache.Set(cacheKey, result, new MemoryCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2),
				SlidingExpiration = TimeSpan.FromMinutes(15)
			});

			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting all categories");
			return new List<CategoryDto>();
		}
	}

	public async Task<List<CategoryDto>> GetAllCategoriesWithChildrenAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			var cacheKey = $"{CacheKey}_tree";

			if (_cache.TryGetValue(cacheKey, out List<CategoryDto>? cached) && cached != null)
				return cached;

			var categories = await _categoryRepository.GetAllWithChildrenAsync(cancellationToken);

			// Build tree structure
			var allCategories = categories.Select(MapToDto).ToList();
			var lookup = allCategories.ToDictionary(c => c.Id);
			var roots = new List<CategoryDto>();

			foreach (var category in allCategories)
			{
				if (category.ParentId.HasValue && lookup.TryGetValue(category.ParentId.Value, out var parent))
				{
					parent.Children.Add(category);
				}
				else
				{
					roots.Add(category);
				}
			}

			_cache.Set(cacheKey, roots, new MemoryCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2),
				SlidingExpiration = TimeSpan.FromMinutes(15)
			});

			return roots;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting category tree");
			return new List<CategoryDto>();
		}
	}

	public async Task<CategoryDto?> GetCategoryBySlugAsync(string slug, CancellationToken cancellationToken = default)
	{
		try
		{
			var cacheKey = $"category_slug_{slug}";

			if (_cache.TryGetValue(cacheKey, out CategoryDto? cached) && cached != null)
				return cached;

			var category = await _categoryRepository.GetBySlugAsync(slug, cancellationToken);
			if (category == null) return null;

			var result = MapToDto(category);

			_cache.Set(cacheKey, result, new MemoryCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
				SlidingExpiration = TimeSpan.FromMinutes(10)
			});

			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting category by slug: {Slug}", slug);
			return null;
		}
	}

	public async Task<CategoryDto?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		try
		{
			var cacheKey = $"category_id_{id}";

			if (_cache.TryGetValue(cacheKey, out CategoryDto? cached) && cached != null)
				return cached;

			var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
			if (category == null) return null;

			var result = MapToDto(category);

			_cache.Set(cacheKey, result, new MemoryCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
				SlidingExpiration = TimeSpan.FromMinutes(10)
			});

			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting category by id: {Id}", id);
			return null;
		}
	}

	public async Task<Guid?> GetCategoryIdBySlugAsync(string slug, CancellationToken cancellationToken = default)
	{
		var category = await GetCategoryBySlugAsync(slug, cancellationToken);
		return category?.Id;
	}

	public async Task ClearCategoryCacheAsync()
	{
		_cache.Remove($"{CacheKey}_list");
		_cache.Remove($"{CacheKey}_tree");
		_logger.LogInformation("Category cache cleared");
	}

	private CategoryDto MapToDto(Category category)
	{
		return new CategoryDto
		{
			Id = category.Id,
			Name = category.Name,
			Slug = category.Slug,
			Description = category.Description,
			ParentId = category.ParentId,
			Children = new List<CategoryDto>()
		};
	}
}