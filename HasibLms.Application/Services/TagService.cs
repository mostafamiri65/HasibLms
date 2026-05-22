using HasibLms.Domain.Interfaces;
using HasibLms.Shared.DTOs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace HasibLms.Application.Services;

public class TagService : ITagService
{
	private readonly ITagRepository _tagRepository;
	private readonly IMemoryCache _cache;
	private readonly ILogger<TagService> _logger;
	private const string CacheKey = "all_tags";

	public TagService(
		ITagRepository tagRepository,
		IMemoryCache cache,
		ILogger<TagService> logger)
	{
		_tagRepository = tagRepository;
		_cache = cache;
		_logger = logger;
	}

	public async Task<List<TagDto>> GetAllTagsAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			if (_cache.TryGetValue(CacheKey, out List<TagDto>? cached) && cached != null)
				return cached;

			var tags = await _tagRepository.GetAllAsync(cancellationToken);
			var result = tags.Select(t => new TagDto
			{
				Id = t.Id,
				Name = t.Name,
				Slug = t.Slug
			}).ToList();

			_cache.Set(CacheKey, result, new MemoryCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2),
				SlidingExpiration = TimeSpan.FromMinutes(15)
			});

			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting all tags");
			return new List<TagDto>();
		}
	}

	public async Task<TagDto?> GetTagBySlugAsync(string slug, CancellationToken cancellationToken = default)
	{
		try
		{
			var cacheKey = $"tag_slug_{slug}";

			if (_cache.TryGetValue(cacheKey, out TagDto? cached) && cached != null)
				return cached;

			var tag = await _tagRepository.GetSingleAsync(t => t.Slug == slug, cancellationToken);
			if (tag == null) return null;

			var result = new TagDto
			{
				Id = tag.Id,
				Name = tag.Name,
				Slug = tag.Slug
			};

			_cache.Set(cacheKey, result, new MemoryCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
				SlidingExpiration = TimeSpan.FromMinutes(10)
			});

			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting tag by slug: {Slug}", slug);
			return null;
		}
	}

	public async Task<TagDto?> GetTagByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		try
		{
			var cacheKey = $"tag_id_{id}";

			if (_cache.TryGetValue(cacheKey, out TagDto? cached) && cached != null)
				return cached;

			var tag = await _tagRepository.GetByIdAsync(id, cancellationToken);
			if (tag == null) return null;

			var result = new TagDto
			{
				Id = tag.Id,
				Name = tag.Name,
				Slug = tag.Slug
			};

			_cache.Set(cacheKey, result, new MemoryCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
				SlidingExpiration = TimeSpan.FromMinutes(10)
			});

			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting tag by id: {Id}", id);
			return null;
		}
	}

	public async Task ClearTagCacheAsync()
	{
		_cache.Remove(CacheKey);
		_logger.LogInformation("Tag cache cleared");
	}
}